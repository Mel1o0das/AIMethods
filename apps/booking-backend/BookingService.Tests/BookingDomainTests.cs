using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using CSharpFunctionalExtensions;

namespace BookingService.Tests;

public class BookingDomainTests
{
    [Fact]
    public void Create_WithValidData_ReturnsBooking()
    {
        Result<Booking, Error> result = CreateBooking(
            new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            2);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithEqualDates_ReturnsValidationError()
    {
        var dateTime = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        Result<Booking, Error> result = CreateBooking(dateTime, dateTime, 2);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.VALIDATION, result.Error.Type);
    }

    [Fact]
    public void Create_WithStartAfterEnd_ReturnsValidationError()
    {
        Result<Booking, Error> result = CreateBooking(
            new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            2);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.VALIDATION, result.Error.Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveGuestCount_ReturnsValidationError(int guestsCount)
    {
        Result<Booking, Error> result = CreateBooking(
            new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            guestsCount);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.VALIDATION, result.Error.Type);
    }

    private static Result<Booking, Error> CreateBooking(
        DateTime startDateTime,
        DateTime endDateTime,
        int guestsCount)
    {
        return Booking.Create(
            UserId.Create(Guid.NewGuid()),
            BookingObjectId.Create(Guid.NewGuid()),
            startDateTime,
            endDateTime,
            guestsCount,
            comment: null,
            BookingStatusId.Create(Guid.NewGuid()));
    }
}
