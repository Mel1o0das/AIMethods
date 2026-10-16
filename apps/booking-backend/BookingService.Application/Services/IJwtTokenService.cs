using BookingService.Domain.Users;

namespace BookingService.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user, string roleName);
}
