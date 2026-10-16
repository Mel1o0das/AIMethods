using BookingService.Application.Features.Users;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Postgres.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly BookingServiceDbContext _dbContext;

        public UserRepository(BookingServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UsersDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Select(u => new UsersDto(u.Id.Value, u.Email, u.FullName, u.Phone, u.Role.Value, u.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<Guid, Error>> AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return user.Id.Value;
        }

        public async Task<Result<User, Error>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

            if (user is null)
                return GeneralError.NotFound(name: "пользователь");

            return user;
        }

        public async Task<Result<User, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == UserId.Create(id), cancellationToken);

            if (user is null)
                return GeneralError.NotFound(id, "пользователь");

            return user;
        }
    }
}
