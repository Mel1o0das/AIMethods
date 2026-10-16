using System.Security.Claims;
using BookingService.Application.Features.Bookings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookingService.Tests;

public class BookingAuthorizationTests
{
    [Fact]
    public async Task CreateWithoutAuthenticatedUser_ReturnsAuthenticationError()
    {
        var handler = new CreateBookingHandler(
            NullLogger<CreateBookingHandler>.Instance,
            new UnusedBookingRepository(),
            new UnusedBookingStatusRepository());
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            2,
            null,
            Guid.NewGuid());

        Result<Guid, Error> result = await handler.Handle(request, new ClaimsPrincipal());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.AUTHENTICATION, result.Error.Type);
    }

    private sealed class UnusedBookingRepository : IBookingRepository
    {
        public Task<List<BookingsDto>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<Result<Guid, Error>> AddIfAvailableAsync(Booking booking, BookingStatusId cancelledStatusId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<List<BookingsDto>> GetMyAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<Result<Guid, Error>> ConfirmAsync(Guid id, BookingStatusId confirmedStatusId, BookingStatusId cancelledStatusId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<Result<Guid, Error>> CancelAsync(Guid id, Guid requesterUserId, bool isAdministrator, BookingStatusId cancelledStatusId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<List<OccupiedSlotDto>> GetOccupiedSlotsAsync(Guid objectId, DateOnly date, BookingStatusId excludedStatusId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<List<OccupiedSlotDto>> GetOccupiedSlotsForRangeAsync(Guid objectId, DateOnly from, DateOnly to, BookingStatusId excludedStatusId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class UnusedBookingStatusRepository : IBookingStatusRepository
    {
        public Task<List<BookingStatusesDto>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<Result<Guid, Error>> AddAsync(BookingStatus bookingStatus) => throw new NotSupportedException();

        public Task<Result<BookingStatusId, Error>> GetByNameAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
