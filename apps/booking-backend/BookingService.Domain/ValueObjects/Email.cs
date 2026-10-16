using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record Email
    {
        private Email(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Email, Error> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return GeneralError.ValueIsRequred("email");

            return new Email(email);
        }
    }
}