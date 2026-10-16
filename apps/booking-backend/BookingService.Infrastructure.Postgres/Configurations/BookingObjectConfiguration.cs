using BookingService.Domain.BookingObjects;
using BookingService.Infrastructure.Postgres.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Postgres.Configurations
{
    public class BookingObjectConfiguration : IEntityTypeConfiguration<BookingObject>
    {
        public void Configure(EntityTypeBuilder<BookingObject> builder)
        {
            builder.ToTable("booking_objects");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    value => BookingObjectId.Create(value))
                .HasColumnName("booking_object_id");

            builder.OwnsOne(o => o.Name, ob =>
            {
                ob.Property(o => o.Value)
                 .HasColumnName("name")
                 .IsRequired()
                 .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);
            });

            builder.OwnsOne(o => o.Description, ob =>
            {
                ob.Property(o => o.Value)
                    .HasColumnName("description")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_HIGH_TEXT_LENGTH);
            });

            builder.Property(o => o.Price)
                .HasColumnName("price")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.Capacity)
                .HasColumnName("capacity")
                .IsRequired();

            builder.Property(o => o.ImageUrl)
                .HasColumnName("image_url")
                .IsRequired(false)
                .HasMaxLength(DbConstants.MAX_URL_LENGTH);

            builder.Property(o => o.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.Property(o => o.IsDeleted)
                .HasColumnName("is_deleted")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(o => o.ObjectTypeId)
                .HasColumnName("object_type_id")
                .IsRequired(false);

            builder.HasQueryFilter(o => !o.IsDeleted);

            builder.HasIndex(o => o.IsActive);
            builder.HasIndex(o => o.ObjectTypeId);
        }
    }
}
