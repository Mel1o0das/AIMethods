using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record Comment
    {
        private Comment(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Comment, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralError.ValueIsInvalid("comment");

            return new Comment(value);
        }
    }
}