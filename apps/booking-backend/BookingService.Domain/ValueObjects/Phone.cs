using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record Phone
    {
        private Phone(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Phone, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralError.ValueIsRequred("phone");

            return new Phone(value);
        }
    }
}