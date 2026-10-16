using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Domain.BookingObjects
{
    public class BookingObject : Shared.Entity<BookingObjectId>
    {
        private BookingObject(BookingObjectId id)
            : base(id)
        {
        }

        private BookingObject(
            BookingObjectId id,
            Name name,
            Description description,
            decimal price,
            int capacity,
            string imageUrl,
            bool isActive,
            Guid? objectTypeId)
            : base(id)
        {
            Name = name;
            Description = description;
            Price = price;
            Capacity = capacity;
            ImageUrl = imageUrl;
            IsActive = isActive;
            ObjectTypeId = objectTypeId;
            CreatedAt = DateTime.UtcNow;
        }

        public Name Name { get; private set; } = null!;

        public Description Description { get; private set; } = null!;

        public decimal Price { get; private set; }

        public int Capacity { get; private set; }

        public string? ImageUrl { get; private set; }

        public bool IsActive { get; private set; }

        public Guid? ObjectTypeId { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public static Result<BookingObject, Error> Create(
            Name name,
            Description description,
            decimal price,
            int capacity,
            string imageUrl,
            bool isActive,
            Guid? objectTypeId = null)
        {
            return new BookingObject(
                BookingObjectId.NewBookingObjectId(),
                name,
                description,
                price,
                capacity,
                imageUrl,
                isActive,
                objectTypeId);
        }

        public void SoftDelete()
        {
            IsDeleted = true;
        }

        public void Update(Name name, Description description, decimal price, int capacity, string? imageUrl, bool isActive, Guid? objectTypeId)
        {
            Name = name;
            Description = description;
            Price = price;
            Capacity = capacity;
            ImageUrl = imageUrl;
            IsActive = isActive;
            ObjectTypeId = objectTypeId;
        }
    }
}