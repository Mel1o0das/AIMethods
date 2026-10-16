using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.Roles
{
    public class Role : Shared.Entity<RoleId>
    {
        private Role(RoleId id)
            : base(id)
        {
        }

        private Role(
            RoleId id,
            Name name)
            : base(id)
        {
            Name = name;
        }

        public Name Name { get; private set; } = null!;

        public static Result<Role, Error> Create(Name name)
        {
            return new Role(RoleId.NewRoleId(), name);
        }
    }
}