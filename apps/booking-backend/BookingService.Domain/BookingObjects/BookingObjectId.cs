namespace BookingService.Domain.BookingObjects
{
    public record BookingObjectId
    {
        private BookingObjectId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static BookingObjectId NewBookingObjectId() => new(Guid.NewGuid());

        public static BookingObjectId EmptyBookingObjectId() => new(Guid.Empty);

        public static BookingObjectId Create(Guid id) => new(id);
    }
}