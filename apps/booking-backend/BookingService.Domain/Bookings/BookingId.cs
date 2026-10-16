namespace BookingService.Domain.Bookings
{
    public record BookingId
    {
        private BookingId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static BookingId NewBookingId() => new(Guid.NewGuid());

        public static BookingId EmptyrBookingId() => new(Guid.Empty);

        public static BookingId Create(Guid id) => new(id);
    }
}