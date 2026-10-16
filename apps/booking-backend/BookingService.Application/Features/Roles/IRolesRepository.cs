using BookingService.Domain.Roles;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.Roles
{
    public interface IRolesRepository
    {
        Task<List<RolesDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<RoleId, Error>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> AddAsync(Role role, CancellationToken cancellationToken = default);
    }
}
