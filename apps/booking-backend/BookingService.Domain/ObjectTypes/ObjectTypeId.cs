namespace BookingService.Domain.ObjectTypes
{
    public record ObjectTypeId
    {
        private ObjectTypeId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static ObjectTypeId NewObjectTypeId() => new(Guid.NewGuid());

        public static ObjectTypeId Create(Guid id) => new(id);
    }
}
