using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.Roles;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Roles;

public record CreateRoleRequest(string Name);

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles", async Task<EndpointResult<Guid>> (
            [FromBody] CreateRoleRequest request,
            [FromServices] CreateRoleHandler handler) =>
                await handler.Handle(request))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class CreateRoleHandler
{
    private readonly ILogger<CreateRoleHandler> _logger;
    private readonly IRolesRepository _rolesRepository;

    public CreateRoleHandler(ILogger<CreateRoleHandler> logger, IRolesRepository rolesRepository)
    {
        _logger = logger;
        _rolesRepository = rolesRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreateRoleRequest request)
    {
        var roleNameResult = Name.Create(request.Name);

        if (roleNameResult.IsFailure)
            return roleNameResult.Error;

        var role = Role.Create(roleNameResult.Value);

        var result = await _rolesRepository.AddAsync(role.Value);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Created role {Id}", role.Value.Id.Value);

        return role.Value.Id.Value;
    }
}
