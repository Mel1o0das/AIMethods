using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.ObjectTypes
{
    public class ObjectType : Shared.Entity<ObjectTypeId>
    {
        private ObjectType(ObjectTypeId id)
            : base(id)
        {
        }

        private ObjectType(ObjectTypeId id, Name name)
            : base(id)
        {
            Name = name;
        }

        public Name Name { get; private set; } = null!;

        public static Result<ObjectType, Error> Create(Name name)
        {
            return new ObjectType(ObjectTypeId.NewObjectTypeId(), name);
        }
    }
}
