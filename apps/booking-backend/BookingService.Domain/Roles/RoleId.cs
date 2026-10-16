namespace BookingService.Domain.Roles
{
    public record RoleId
    {
        private RoleId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static RoleId NewRoleId() => new(Guid.NewGuid());

        public static RoleId EmptyrRoleId() => new(Guid.Empty);

        public static RoleId Create(Guid id) => new(id);
    }
}