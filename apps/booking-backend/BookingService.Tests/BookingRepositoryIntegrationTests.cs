using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using BookingService.Infrastructure.Postgres;
using BookingService.Infrastructure.Postgres.Repositories;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingService.Tests;

public class BookingRepositoryIntegrationTests
{
    private static readonly DateTime _startDateTime = new(2035, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithFreeInterval_CreatesBooking()
    {
        TestData data = await SeedAsync();
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime, _startDateTime.AddHours(2), 2);

        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);

        Assert.True(result.IsSuccess);
    }

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithSameInterval_ReturnsConflict()
    {
        TestData data = await SeedAsync();
        await AddBookingAsync(data, _startDateTime, _startDateTime.AddHours(2));
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime, _startDateTime.AddHours(2), 2);

        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.CONFLICT, result.Error.Type);
    }

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithPartialOverlap_ReturnsConflict()
    {
        TestData data = await SeedAsync();
        await AddBookingAsync(data, _startDateTime, _startDateTime.AddHours(2));
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime.AddHours(1), _startDateTime.AddHours(3), 2);

        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.CONFLICT, result.Error.Type);
    }

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithTouchingInterval_CreatesBooking()
    {
        TestData data = await SeedAsync();
        await AddBookingAsync(data, _startDateTime, _startDateTime.AddHours(2));
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime.AddHours(2), _startDateTime.AddHours(4), 2);

        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);

        Assert.True(result.IsSuccess);
    }

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithCancelledBooking_CreatesBooking()
    {
        TestData data = await SeedAsync();
        await AddBookingAsync(
            data,
            _startDateTime,
            _startDateTime.AddHours(2),
            data.CancelledStatusId);
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime, _startDateTime.AddHours(2), 2);

        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);

        Assert.True(result.IsSuccess);
    }

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithGuestCountOverCapacity_ReturnsValidationError()
    {
        TestData data = await SeedAsync(capacity: 2);
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime, _startDateTime.AddHours(2), 3);

        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.VALIDATION, result.Error.Type);
    }

    [PostgreSqlFact]
    public async Task Confirm_AfterCancellation_ReturnsConflict()
    {
        TestData data = await SeedAsync();
        Guid bookingId = await AddBookingAsync(data, _startDateTime, _startDateTime.AddHours(2));
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);

        Result<Guid, Error> cancelResult = await repository.CancelAsync(
            bookingId,
            data.UserId.Value,
            isAdministrator: false,
            data.CancelledStatusId);
        Result<Guid, Error> confirmResult = await repository.ConfirmAsync(
            bookingId,
            data.ConfirmedStatusId,
            data.CancelledStatusId);

        Assert.True(cancelResult.IsSuccess);
        Assert.True(confirmResult.IsFailure);
        Assert.Equal(ErrorType.CONFLICT, confirmResult.Error.Type);
    }

    [PostgreSqlFact]
    public async Task AddIfAvailable_WithConcurrentRequests_CreatesExactlyOneBooking()
    {
        TestData data = await SeedAsync();
        var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Result<Guid, Error>> firstTask = AddConcurrentlyAsync(data, startGate.Task);
        Task<Result<Guid, Error>> secondTask = AddConcurrentlyAsync(data, startGate.Task);
        startGate.SetResult();

        Result<Guid, Error>[] results = await Task.WhenAll(firstTask, secondTask);

        Assert.Single(results, result => result.IsSuccess);
        Result<Guid, Error> conflict = Assert.Single(results, result => result.IsFailure);
        Assert.Equal(ErrorType.CONFLICT, conflict.Error.Type);
    }

    private static async Task<Result<Guid, Error>> AddConcurrentlyAsync(TestData data, Task startTask)
    {
        await startTask;
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, _startDateTime, _startDateTime.AddHours(2), 2);
        return await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);
    }

    private static async Task<Guid> AddBookingAsync(
        TestData data,
        DateTime startDateTime,
        DateTime endDateTime,
        BookingStatusId? statusId = null)
    {
        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        var repository = new BookingRepository(context);
        Booking booking = CreateBooking(data, startDateTime, endDateTime, 2, statusId);
        Result<Guid, Error> result = await repository.AddIfAvailableAsync(booking, data.CancelledStatusId);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static Booking CreateBooking(
        TestData data,
        DateTime startDateTime,
        DateTime endDateTime,
        int guestsCount,
        BookingStatusId? statusId = null)
    {
        Result<Booking, Error> result = Booking.Create(
            data.UserId,
            data.BookingObjectId,
            startDateTime,
            endDateTime,
            guestsCount,
            comment: null,
            statusId ?? data.PendingStatusId);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static async Task<TestData> SeedAsync(int capacity = 10)
    {
        await PostgreSqlTestDatabase.EnsureMigratedAsync();

        var data = new TestData(
            UserId.Create(Guid.NewGuid()),
            BookingObjectId.Create(Guid.NewGuid()),
            BookingStatusId.Create(Guid.NewGuid()),
            BookingStatusId.Create(Guid.NewGuid()),
            BookingStatusId.Create(Guid.NewGuid()));
        Guid roleId = Guid.NewGuid();
        string suffix = Guid.NewGuid().ToString("N");

        object[] parameters =
        [
            new NpgsqlParameter("roleId", roleId),
            new NpgsqlParameter("roleName", $"Test role {suffix}"),
            new NpgsqlParameter("userId", data.UserId.Value),
            new NpgsqlParameter("email", $"{suffix}@example.test"),
            new NpgsqlParameter("phone", $"+7{suffix[..10]}"),
            new NpgsqlParameter("bookingObjectId", data.BookingObjectId.Value),
            new NpgsqlParameter("pendingStatusId", data.PendingStatusId.Value),
            new NpgsqlParameter("cancelledStatusId", data.CancelledStatusId.Value),
            new NpgsqlParameter("confirmedStatusId", data.ConfirmedStatusId.Value),
            new NpgsqlParameter("pendingName", $"Pending {suffix}"),
            new NpgsqlParameter("cancelledName", $"Cancelled {suffix}"),
            new NpgsqlParameter("confirmedName", $"Confirmed {suffix}"),
            new NpgsqlParameter("capacity", capacity),
            new NpgsqlParameter("createdAt", DateTime.UtcNow),
        ];

        await using BookingServiceDbContext context = PostgreSqlTestDatabase.CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO roles (role_id, name)
            VALUES (@roleId, @roleName);

            INSERT INTO users (
                user_id, email, password_hash, last_name, first_name, patronymic,
                phone, role_id, created_at)
            VALUES (
                @userId, @email, 'test-hash', 'Test', 'Test', 'Test',
                @phone, @roleId, @createdAt);

            INSERT INTO booking_statuses (booking_status_id, name)
            VALUES
                (@pendingStatusId, @pendingName),
                (@cancelledStatusId, @cancelledName),
                (@confirmedStatusId, @confirmedName);

            INSERT INTO booking_objects (
                booking_object_id, name, description, price, capacity, image_url,
                is_active, is_deleted, object_type_id, created_at)
            VALUES (
                @bookingObjectId, 'Test object', 'Test description', 100, @capacity, NULL,
                TRUE, FALSE, NULL, @createdAt);
            """,
            parameters);

        return data;
    }

    private sealed record TestData(
        UserId UserId,
        BookingObjectId BookingObjectId,
        BookingStatusId PendingStatusId,
        BookingStatusId CancelledStatusId,
        BookingStatusId ConfirmedStatusId);
}
