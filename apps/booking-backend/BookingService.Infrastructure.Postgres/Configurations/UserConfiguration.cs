using BookingService.Domain.Roles;
using BookingService.Domain.Users;
using BookingService.Infrastructure.Postgres.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Postgres.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasConversion(
                    id => id.Value,
                    value => UserId.Create(value))
                .HasColumnName("user_id");

            builder.OwnsOne(u => u.Email, ub =>
            {
                ub.Property(u => u.Value)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_EMAIL_LENGTH);

                ub.HasIndex(s => s.Value)
                    .IsUnique();
            });

            builder.OwnsOne(u => u.PasswordHash, ub =>
            {
                ub.Property(u => u.Value)
                    .HasColumnName("password_hash")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_PASSWORD_HASH_LENGTH);
            });

            builder.OwnsOne(u => u.FullName, fullName =>
            {
                fullName.Property(f => f.LastName)
                    .HasColumnName("last_name")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);

                fullName.Property(f => f.FirstName)
                    .HasColumnName("first_name")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);

                fullName.Property(f => f.Patronymic)
                    .HasColumnName("patronymic")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);
            });

            builder.OwnsOne(u => u.Phone, ub =>
            {
                ub.Property(u => u.Value)
                    .HasColumnName("phone")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_PHONE_LENGTH);

                ub.HasIndex(s => s.Value)
                    .IsUnique();
            });

            builder.Property(u => u.Role)
                .HasConversion(
                    id => id.Value,
                    value => RoleId.Create(value))
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.HasIndex(u => u.Role);

            builder.HasOne<Role>()
                .WithMany()
                .HasForeignKey(u => u.Role)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
