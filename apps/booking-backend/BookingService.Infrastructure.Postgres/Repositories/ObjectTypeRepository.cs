using BookingService.Application.Features.ObjectTypes;
using BookingService.Domain.ObjectTypes;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class ObjectTypeRepository : IObjectTypeRepository
    {
        private readonly BookingServiceDbContext _dbContext;

        public ObjectTypeRepository(BookingServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ObjectTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ObjectTypes
                .AsNoTracking()
                .Select(t => new ObjectTypeDto(t.Id.Value, t.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<Guid, Error>> AddAsync(ObjectType objectType)
        {
            await _dbContext.ObjectTypes.AddAsync(objectType);
            await _dbContext.SaveChangesAsync();

            return objectType.Id.Value;
        }
    }
}
