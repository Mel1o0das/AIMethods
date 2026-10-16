using BookingService.Domain.ObjectTypes;
using BookingService.Infrastructure.Postgres.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Postgres.Configurations
{
    public class ObjectTypeConfiguration : IEntityTypeConfiguration<ObjectType>
    {
        public void Configure(EntityTypeBuilder<ObjectType> builder)
        {
            builder.ToTable("object_types");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasConversion(
                    id => id.Value,
                    value => ObjectTypeId.Create(value))
                .HasColumnName("object_type_id");

            builder.OwnsOne(t => t.Name, tb =>
            {
                tb.Property(t => t.Value)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);

                tb.HasIndex(t => t.Value)
                    .IsUnique();
            });
        }
    }
}
