using BookingService.Domain.BookingStatuses;
using BookingService.Infrastructure.Postgres.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Postgres.Configurations
{
    public class BookingStatusConfiguration : IEntityTypeConfiguration<BookingStatus>
    {
        public void Configure(EntityTypeBuilder<BookingStatus> builder)
        {
            builder.ToTable("booking_statuses");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasConversion(
                    id => id.Value,
                    value => BookingStatusId.Create(value))
                .HasColumnName("booking_status_id");

            builder.OwnsOne(bs => bs.Name, bsb =>
            {
                bsb.Property(s => s.Value)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);

                bsb.HasIndex(s => s.Value)
                    .IsUnique();
            });
        }
    }
}
