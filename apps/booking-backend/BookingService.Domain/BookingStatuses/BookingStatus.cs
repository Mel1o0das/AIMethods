using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.BookingStatuses
{
    public class BookingStatus : Shared.Entity<BookingStatusId>
    {
        private BookingStatus(BookingStatusId id)
            : base(id)
        {
        }

        private BookingStatus(
            BookingStatusId id,
            Name name)
            : base(id)
        {
            Name = name;
        }

        public Name Name { get; private set; } = null!;

        public static Result<BookingStatus, Error> Create(Name name)
        {
            return new BookingStatus(BookingStatusId.NewBookingStatusId(), name);
        }
    }
}