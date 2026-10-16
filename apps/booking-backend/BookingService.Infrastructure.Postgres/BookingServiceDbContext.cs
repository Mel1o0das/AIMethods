using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.ObjectTypes;
using BookingService.Domain.Roles;
using BookingService.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres
{
    public class BookingServiceDbContext : DbContext
    {
        private readonly string _connectionString;

        public BookingServiceDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingServiceDbContext).Assembly);
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<BookingObject> BookingObjects => Set<BookingObject>();
        public DbSet<BookingStatus> BookingStatuses => Set<BookingStatus>();
        public DbSet<ObjectType> ObjectTypes => Set<ObjectType>();
        public DbSet<Booking> Bookings => Set<Booking>();
    }
}