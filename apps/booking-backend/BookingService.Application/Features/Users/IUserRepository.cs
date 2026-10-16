using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.Users
{
    public interface IUserRepository
    {
        Task<List<UsersDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> AddAsync(User user);
        Task<Result<User, Error>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<User, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
