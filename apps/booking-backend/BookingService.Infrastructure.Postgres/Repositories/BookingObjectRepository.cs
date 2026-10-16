using BookingService.Application.Features.BookingObjects;
using BookingService.Domain.BookingObjects;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class BookingObjectRepository : IBookingObjectRepository
    {
        private readonly BookingServiceDbContext _dbContext;

        public BookingObjectRepository(BookingServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<BookingObjectsDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.BookingObjects
                .AsNoTracking()
                .Select(bo => new BookingObjectsDto(bo.Id.Value, bo.Name, bo.Description, bo.Price, bo.Capacity, bo.ImageUrl, bo.IsActive, bo.CreatedAt, bo.ObjectTypeId))
                .ToListAsync(cancellationToken);
        }

        public async Task<BookingObjectsPagedResult> GetPagedAsync(BookingObjectsPagedQuery query, CancellationToken cancellationToken = default)
        {
            var q = _dbContext.BookingObjects.AsNoTracking();

            if (!query.IncludeInactive)
                q = q.Where(bo => bo.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var pattern = $"%{query.Search}%";
                q = q.Where(bo =>
                    EF.Functions.ILike(bo.Name.Value, pattern) ||
                    EF.Functions.ILike(bo.Description.Value, pattern));
            }

            if (query.TypeId.HasValue)
                q = q.Where(bo => bo.ObjectTypeId == query.TypeId.Value);

            if (query.MaxPrice.HasValue)
                q = q.Where(bo => bo.Price <= query.MaxPrice.Value);

            if (query.MinCapacity.HasValue)
                q = q.Where(bo => bo.Capacity >= query.MinCapacity.Value);

            q = (query.SortBy?.ToLowerInvariant(), query.SortDir?.ToLowerInvariant()) switch
            {
                ("price", "desc") => q.OrderByDescending(bo => bo.Price),
                ("price", _) => q.OrderBy(bo => bo.Price),
                ("capacity", "desc") => q.OrderByDescending(bo => bo.Capacity),
                ("capacity", _) => q.OrderBy(bo => bo.Capacity),
                ("name", "desc") => q.OrderByDescending(bo => bo.Name.Value),
                ("name", _) => q.OrderBy(bo => bo.Name.Value),
                _ => q.OrderBy(bo => bo.CreatedAt),
            };

            var totalCount = await q.CountAsync(cancellationToken);
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / query.PageSize);

            var items = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(bo => new BookingObjectsDto(bo.Id.Value, bo.Name, bo.Description, bo.Price, bo.Capacity, bo.ImageUrl, bo.IsActive, bo.CreatedAt, bo.ObjectTypeId))
                .ToListAsync(cancellationToken);

            return new BookingObjectsPagedResult(items, totalCount, query.Page, query.PageSize, totalPages);
        }

        public async Task<Result<BookingObjectsDto, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var dto = await _dbContext.BookingObjects
                .AsNoTracking()
                .Where(bo => bo.Id == BookingObjectId.Create(id))
                .Select(bo => new BookingObjectsDto(bo.Id.Value, bo.Name, bo.Description, bo.Price, bo.Capacity, bo.ImageUrl, bo.IsActive, bo.CreatedAt, bo.ObjectTypeId))
                .FirstOrDefaultAsync(cancellationToken);

            if (dto is null)
                return GeneralError.NotFound(id, "объект бронирования");

            return dto;
        }

        public async Task<Result<Guid, Error>> UpdateAsync(Guid id, Name name, Description description, decimal price, int capacity, string? imageUrl, bool isActive, Guid? objectTypeId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.BookingObjects
                .FirstOrDefaultAsync(bo => bo.Id == BookingObjectId.Create(id), cancellationToken);

            if (entity is null)
                return GeneralError.NotFound(id, "объект бронирования");

            entity.Update(name, description, price, capacity, imageUrl, isActive, objectTypeId);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity.Id.Value;
        }

        public async Task<Result<Guid, Error>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.BookingObjects
                .FirstOrDefaultAsync(bo => bo.Id == BookingObjectId.Create(id), cancellationToken);

            if (entity is null)
                return GeneralError.NotFound(id, "объект бронирования");

            entity.SoftDelete();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity.Id.Value;
        }

        public async Task<Result<Guid, Error>> AddAsync(BookingObject bookingObject)
        {
            await _dbContext.BookingObjects.AddAsync(bookingObject);
            await _dbContext.SaveChangesAsync();

            return bookingObject.Id.Value;
        }
    }
}
