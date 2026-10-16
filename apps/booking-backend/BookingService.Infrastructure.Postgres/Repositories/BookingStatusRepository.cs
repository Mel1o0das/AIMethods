using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class BookingStatusRepository : IBookingStatusRepository
    {
        private readonly BookingServiceDbContext _dbContext;

        public BookingStatusRepository(BookingServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<BookingStatusesDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.BookingStatuses
                .AsNoTracking()
                .Select(bs => new BookingStatusesDto(bs.Id.Value, bs.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<Guid, Error>> AddAsync(BookingStatus bookingStatus)
        {
            await _dbContext.BookingStatuses.AddAsync(bookingStatus);
            await _dbContext.SaveChangesAsync();

            return bookingStatus.Id.Value;
        }

        public async Task<Result<BookingStatusId, Error>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var status = await _dbContext.BookingStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name.Value == name, cancellationToken);

            if (status is null)
                return GeneralError.NotFound(name: $"статус '{name}'");

            return status.Id;
        }
    }
}
