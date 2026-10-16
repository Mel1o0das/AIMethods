using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Users;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Auth;

public record UserProfileDto(
    Guid Id,
    string Email,
    string LastName,
    string FirstName,
    string Patronymic,
    string Phone,
    Guid RoleId,
    DateTime CreatedAt);

public class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/me", async Task<EndpointResult<UserProfileDto>> (
            ClaimsPrincipal principal,
            [FromServices] GetMeHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class GetMeHandler
{
    private readonly ILogger<GetMeHandler> _logger;
    private readonly IUserRepository _userRepository;

    public GetMeHandler(ILogger<GetMeHandler> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto, Error>> Handle(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var userIdString = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId))
            return Error.Authentication("auth.invalid_token", "Недействительный токен");

        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (userResult.IsFailure)
            return userResult.Error;

        var user = userResult.Value;

        _logger.LogInformation("Get profile for user {Id}", userId);

        return new UserProfileDto(
            user.Id.Value,
            user.Email.Value,
            user.FullName.LastName,
            user.FullName.FirstName,
            user.FullName.Patronymic,
            user.Phone.Value,
            user.Role.Value,
            user.CreatedAt);
    }
}
