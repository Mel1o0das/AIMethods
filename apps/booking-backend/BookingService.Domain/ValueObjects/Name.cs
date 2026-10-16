using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record Name
    {
        private Name(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Name, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralError.ValueIsRequred("name");

            return new Name(value);
        }
    }
}