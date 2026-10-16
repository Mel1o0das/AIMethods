using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Users;

public record UsersDto(Guid Id, Email Email, FullName FullName, Phone Phone, Guid RoleId, DateTime CreatedAt);

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async Task<EndpointResult<IReadOnlyList<UsersDto>>> (
            [FromServices] GetUserHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(cancellationToken))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class GetUserHandler
{
    private readonly ILogger<GetUserHandler> _logger;
    private readonly IUserRepository _userRepository;

    public GetUserHandler(ILogger<GetUserHandler> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UsersDto>, Error>> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get all users");

        return await _userRepository.GetAllAsync(cancellationToken);
    }
}
