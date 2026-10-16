using System.Data;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private const string LockBookingObjectSql = """
            SELECT booking_object_id AS "Value"
            FROM booking_objects
            WHERE booking_object_id = @bookingObjectId
            FOR UPDATE
            """;

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

        public async Task<List<BookingsDto>> GetMyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == UserId.Create(id))
                .Select(b => new BookingsDto(b.Id.Value, b.UserId.Value, b.BookingObjectId.Value, b.StartDateTime, b.EndDateTime, b.GuestsCount, b.Comment, b.Status.Value, b.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<Guid, Error>> AddIfAvailableAsync(
            Booking booking,
            BookingStatusId cancelledStatusId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var objectExists = await LockBookingObjectAsync(booking.BookingObjectId, cancellationToken);
            if (!objectExists)
                return GeneralError.NotFound(booking.BookingObjectId.Value, "объект бронирования");

            var bookingObject = await _dbContext.BookingObjects
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstAsync(o => o.Id == booking.BookingObjectId, cancellationToken);

            if (bookingObject.IsDeleted)
                return GeneralError.NotFound(booking.BookingObjectId.Value, "объект бронирования");

            if (!bookingObject.IsActive)
            {
                return Error.Conflict(
                    "booking_object.inactive",
                    "Объект бронирования недоступен");
            }

            if (booking.GuestsCount > bookingObject.Capacity)
            {
                return Error.Validation(
                    "booking.guests_count.exceeds_capacity",
                    "Количество гостей превышает вместимость объекта",
                    nameof(booking.GuestsCount));
            }

            var hasOverlap = await HasOverlapAsync(
                booking.BookingObjectId,
                booking.StartDateTime,
                booking.EndDateTime,
                cancelledStatusId,
                excludedBookingId: null,
                cancellationToken);

            if (hasOverlap)
            {
                return Error.Conflict(
                    "booking.interval.conflict",
                    "Выбранный интервал уже занят");
            }

            await _dbContext.Bookings.AddAsync(booking, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return booking.Id.Value;
        }

        public async Task<Result<Guid, Error>> ConfirmAsync(
            Guid id,
            BookingStatusId confirmedStatusId,
            BookingStatusId cancelledStatusId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var bookingId = BookingId.Create(id);
            var bookingObjectId = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Id == bookingId)
                .Select(b => b.BookingObjectId)
                .FirstOrDefaultAsync(cancellationToken);

            if (bookingObjectId is null)
                return GeneralError.NotFound(id, "бронирование");

            await LockBookingObjectAsync(bookingObjectId, cancellationToken);

            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

            if (booking is null)
                return GeneralError.NotFound(id, "бронирование");

            if (booking.Status == cancelledStatusId)
            {
                return Error.Conflict(
                    "booking.status_transition.invalid",
                    "Отменённое бронирование нельзя подтвердить повторно");
            }

            var hasOverlap = await HasOverlapAsync(
                booking.BookingObjectId,
                booking.StartDateTime,
                booking.EndDateTime,
                cancelledStatusId,
                booking.Id,
                cancellationToken);

            if (hasOverlap)
            {
                return Error.Conflict(
                    "booking.interval.conflict",
                    "Выбранный интервал уже занят");
            }

            booking.SetStatus(confirmedStatusId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return booking.Id.Value;
        }

        public async Task<Result<Guid, Error>> CancelAsync(
            Guid id,
            Guid requesterUserId,
            bool isAdministrator,
            BookingStatusId cancelledStatusId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var bookingId = BookingId.Create(id);
            var bookingObjectId = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Id == bookingId)
                .Select(b => b.BookingObjectId)
                .FirstOrDefaultAsync(cancellationToken);

            if (bookingObjectId is null)
                return GeneralError.NotFound(id, "бронирование");

            await LockBookingObjectAsync(bookingObjectId, cancellationToken);

            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

            if (booking is null)
                return GeneralError.NotFound(id, "бронирование");

            if (!isAdministrator && booking.UserId != UserId.Create(requesterUserId))
            {
                return Error.Authorization(
                    "booking.cancel.forbidden",
                    "Недостаточно прав для отмены бронирования");
            }

            booking.SetStatus(cancelledStatusId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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

        private async Task<bool> LockBookingObjectAsync(
            BookingObjectId bookingObjectId,
            CancellationToken cancellationToken)
        {
            var parameter = new NpgsqlParameter("bookingObjectId", bookingObjectId.Value);
            var lockedObjectIds = await _dbContext.Database
                .SqlQueryRaw<Guid>(LockBookingObjectSql, parameter)
                .ToListAsync(cancellationToken);

            return lockedObjectIds.Count != 0;
        }

        private Task<bool> HasOverlapAsync(
            BookingObjectId bookingObjectId,
            DateTime startDateTime,
            DateTime endDateTime,
            BookingStatusId cancelledStatusId,
            BookingId? excludedBookingId,
            CancellationToken cancellationToken)
        {
            return _dbContext.Bookings.AnyAsync(
                b => b.BookingObjectId == bookingObjectId
                    && b.Status != cancelledStatusId
                    && (excludedBookingId == null || b.Id != excludedBookingId)
                    && b.StartDateTime < endDateTime
                    && b.EndDateTime > startDateTime,
                cancellationToken);
        }
    }
}
