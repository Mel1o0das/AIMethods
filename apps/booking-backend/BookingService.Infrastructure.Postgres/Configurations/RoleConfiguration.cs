using BookingService.Domain.Roles;
using BookingService.Infrastructure.Postgres.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Postgres.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasConversion(
                    id => id.Value,
                    value => RoleId.Create(value))
                .HasColumnName("role_id");

            builder.OwnsOne(r => r.Name, rb =>
            {
                rb.Property(r => r.Value)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(DbConstants.MAX_LOW_TEXT_LENGTH);

                rb.HasIndex(r => r.Value)
                    .IsUnique();
            });
        }
    }
}
