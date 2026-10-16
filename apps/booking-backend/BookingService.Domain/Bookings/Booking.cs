using BookingService.Domain.BookingObjects;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.Bookings
{
    public class Booking : Shared.Entity<BookingId>
    {
        private Booking(BookingId id)
            : base(id)
        {
        }

        private Booking(
            BookingId id,
            UserId userId,
            BookingObjectId bookingObjectId,
            DateTime startDateTime,
            DateTime endDateTime,
            int guestsCount,
            Comment? comment,
            BookingStatusId status)
            : base(id)
        {
            UserId = userId;
            BookingObjectId = bookingObjectId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            GuestsCount = guestsCount;
            Comment = comment;
            Status = status;
            CreatedAt = DateTime.UtcNow;

        }

        public UserId UserId { get; private set; } = null!;

        public BookingObjectId BookingObjectId { get; private set; } = null!;

        public DateTime StartDateTime { get; private set; }

        public DateTime EndDateTime { get; private set; }

        public int GuestsCount { get; private set; }

        public Comment? Comment { get; private set; }

        public BookingStatusId Status { get; private set; } = null!;

        public DateTime CreatedAt { get; private set; }

        public static Result<Booking, Error> Create(
            UserId userId,
            BookingObjectId bookingObjectId,
            DateTime startDateTime,
            DateTime endDateTime,
            int guestsCount,
            Comment? comment,
            BookingStatusId status)
        {
            if (startDateTime >= endDateTime)
            {
                return Error.Validation(
                    "booking.interval.invalid",
                    "Дата начала бронирования должна быть раньше даты окончания",
                    nameof(startDateTime));
            }

            if (guestsCount <= 0)
            {
                return Error.Validation(
                    "booking.guests_count.invalid",
                    "Количество гостей должно быть положительным",
                    nameof(guestsCount));
            }

            return new Booking(
                BookingId.NewBookingId(),
                userId,
                bookingObjectId,
                startDateTime,
                endDateTime,
                guestsCount,
                comment,
                status);
        }

        public void SetStatus(BookingStatusId status)
        {
            Status = status;
        }
    }
}
