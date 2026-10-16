using BookingService.Application.Features.Bookings;
using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingServiceDbContext _dbContext;

        public BookingRepository(BookingServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<BookingsDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Bookings
                .AsNoTracking()
                .Select(b => new BookingsDto(b.Id.Value, b.UserId.Value, b.BookingObjectId.Value, b.StartDateTime, b.EndDateTime, b.GuestsCount, b.Comment, b.Status.Value, b.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<BookingsDto, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var booking = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Id == BookingId.Create(id))
                .Select(b => new BookingsDto(b.Id.Value, b.UserId.Value, b.BookingObjectId.Value, b.StartDateTime, b.EndDateTime, b.GuestsCount, b.Comment, b.Status.Value, b.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            if (booking is null)
                return GeneralError.NotFound(id, "booking");

            return booking;
        }

        public async Task<List<BookingsDto>> GetMyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == UserId.Create(id))
                .Select(b => new BookingsDto(b.Id.Value, b.UserId.Value, b.BookingObjectId.Value, b.StartDateTime, b.EndDateTime, b.GuestsCount, b.Comment, b.Status.Value, b.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<Guid, Error>> AddAsync(Booking booking)
        {
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            return booking.Id.Value;
        }

        public async Task<Result<Guid, Error>> UpdateStatusAsync(Guid id, BookingStatusId statusId, CancellationToken cancellationToken = default)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == BookingId.Create(id), cancellationToken);

            if (booking is null)
                return GeneralError.NotFound(id, "бронирование");

            booking.SetStatus(statusId);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return booking.Id.Value;
        }

        public async Task<List<OccupiedSlotDto>> GetOccupiedSlotsAsync(
            Guid objectId,
            DateOnly date,
            BookingStatusId excludedStatusId,
            CancellationToken cancellationToken = default)
        {
            var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var nextDayStart = dayStart.AddDays(1);

            return await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.BookingObjectId == BookingObjectId.Create(objectId)
                    && b.Status != excludedStatusId
                    && b.StartDateTime < nextDayStart
                    && b.EndDateTime > dayStart)
                .Select(b => new OccupiedSlotDto(b.StartDateTime, b.EndDateTime))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<OccupiedSlotDto>> GetOccupiedSlotsForRangeAsync(
            Guid objectId,
            DateOnly from,
            DateOnly to,
            BookingStatusId excludedStatusId,
            CancellationToken cancellationToken = default)
        {
            var rangeStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var rangeEnd = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            return await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.BookingObjectId == BookingObjectId.Create(objectId)
                    && b.Status != excludedStatusId
                    && b.StartDateTime < rangeEnd
                    && b.EndDateTime > rangeStart)
                .Select(b => new OccupiedSlotDto(b.StartDateTime, b.EndDateTime))
                .ToListAsync(cancellationToken);
        }
    }
}
