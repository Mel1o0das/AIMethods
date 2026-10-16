using BookingService.Application.Features.Roles;
using BookingService.Domain.Roles;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class RoleRepository : IRolesRepository
    {
        private readonly BookingServiceDbContext _dbContext;

        public RoleRepository(BookingServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RolesDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .AsNoTracking()
                .Select(r => new RolesDto(r.Id.Value, r.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<RoleId, Error>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var roleId = await _dbContext.Roles
                .AsNoTracking()
                .Where(r => r.Name.Value == name)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return roleId is null
                ? Error.Failure("role.required_not_configured", $"Required role '{name}' is not configured")
                : roleId;
        }

        public async Task<Result<Guid, Error>> AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            await _dbContext.Roles.AddAsync(role, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return role.Id.Value;
        }
    }
}
