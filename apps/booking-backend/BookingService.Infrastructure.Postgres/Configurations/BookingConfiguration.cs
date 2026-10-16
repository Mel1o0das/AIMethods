using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Users;
using BookingService.Infrastructure.Postgres.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Postgres.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_bookings_start_before_end",
                    "start_date_time < end_date_time");
                tableBuilder.HasCheckConstraint(
                    "CK_bookings_guests_count_positive",
                    "guests_count > 0");
            });

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasConversion(
                    id => id.Value,
                    value => BookingId.Create(value))
                .HasColumnName("booking_id");

            builder.Property(b => b.UserId)
                .HasConversion(
                    id => id.Value,
                    value => UserId.Create(value))
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(b => b.BookingObjectId)
                .HasConversion(
                    id => id.Value,
                    value => BookingObjectId.Create(value))
                .HasColumnName("booking_object_id")
                .IsRequired();

            builder.Property(b => b.StartDateTime)
                .HasColumnName("start_date_time")
                .IsRequired();

            builder.Property(b => b.EndDateTime)
                .HasColumnName("end_date_time")
                .IsRequired();

            builder.Property(b => b.GuestsCount)
                .HasColumnName("guests_count")
                .IsRequired();

            builder.OwnsOne(b => b.Comment, bb =>
            {
                bb.Property(b => b.Value)
                    .HasColumnName("comment")
                    .IsRequired(false)
                    .HasMaxLength(DbConstants.MAX_MEDIUM_TEXT_LENGTH);
            });

            builder.Property(b => b.Status)
                .HasConversion(
                    id => id.Value,
                    value => BookingStatusId.Create(value))
                .HasColumnName("status_id")
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.BookingObjectId);
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => new { b.BookingObjectId, b.StartDateTime, b.EndDateTime });

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BookingObject>()
                .WithMany()
                .HasForeignKey(b => b.BookingObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BookingStatus>()
                .WithMany()
                .HasForeignKey(b => b.Status)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
