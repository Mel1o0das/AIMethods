using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record PasswordHash
    {
        private PasswordHash(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<PasswordHash, Error> Create(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return GeneralError.ValueIsRequred("password");

            if (password.Length < 8)
                return GeneralError.ValueIsInvalid("password");

            return new PasswordHash(password);
        }

        public static PasswordHash FromHash(string hash) => new(hash);
    }
}