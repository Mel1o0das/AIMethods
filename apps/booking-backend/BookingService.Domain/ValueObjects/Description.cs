using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record Description
    {
        private Description(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Description, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralError.ValueIsRequred("description");

            if (value.Length > 2000)
                return GeneralError.ValueIsInvalid("description");

            return new Description(value);
        }
    }
}