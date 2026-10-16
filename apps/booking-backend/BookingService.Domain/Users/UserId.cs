namespace BookingService.Domain.Users
{
    public record UserId
    {
        private UserId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static UserId NewUserId() => new(Guid.NewGuid());

        public static UserId EmptyUserId() => new(Guid.Empty);

        public static UserId Create(Guid id) => new(id);
    }
}