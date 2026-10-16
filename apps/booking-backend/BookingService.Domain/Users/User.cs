using BookingService.Domain.Roles;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.Users
{
    public class User : Shared.Entity<UserId>
    {
        private User(UserId id)
            : base(id)
        {
        }

        private User(
            UserId id,
            Email email,
            PasswordHash passwordHash,
            FullName fullName,
            Phone phone,
            RoleId role)
            : base(id)
        {
            Email = email;
            PasswordHash = passwordHash;
            FullName = fullName;
            Phone = phone;
            Role = role;
        }

        public Email Email { get; private set; } = null!;

        public PasswordHash PasswordHash { get; private set; } = null!;

        public FullName FullName { get; private set; } = null!;

        public Phone Phone { get; private set; } = null!;

        public RoleId Role { get; private set; } = null!;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public static Result<User, Error> Create(
            Email email,
            PasswordHash passwordHash,
            FullName fullName,
            Phone phone,
            RoleId role)
        {
            return new User(
                UserId.NewUserId(),
                email,
                passwordHash,
                fullName,
                phone,
                role);
        }
    }
}