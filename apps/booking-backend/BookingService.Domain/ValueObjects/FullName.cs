using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ValueObjects
{
    public record FullName
    {
        private FullName(string lastName, string firstName, string patronymic)
        {
            LastName = lastName;
            FirstName = firstName;
            Patronymic = patronymic;
        }

        public string LastName { get; }

        public string FirstName { get; }

        public string Patronymic { get; }

        public static Result<FullName, Error> Create(string lastName, string firstName, string patronymic)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return GeneralError.ValueIsRequred("lastName");

            if (string.IsNullOrWhiteSpace(firstName))
                return GeneralError.ValueIsRequred("firstName");

            if (string.IsNullOrWhiteSpace(patronymic))
                return GeneralError.ValueIsRequred("patronymic");

            return new FullName(lastName, firstName, patronymic);
        }
    }
}