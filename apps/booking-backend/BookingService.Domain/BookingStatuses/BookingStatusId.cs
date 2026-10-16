namespace BookingService.Domain.BookingStatuses
{
    public record BookingStatusId
    {
        private BookingStatusId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static BookingStatusId NewBookingStatusId() => new(Guid.NewGuid());

        public static BookingStatusId EmptyrBookingStatusId() => new(Guid.Empty);

        public static BookingStatusId Create(Guid id) => new(id);
    }
}