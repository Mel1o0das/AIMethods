using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Roles;

public record RolesDto(Guid id, Name Name);

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles", async Task<EndpointResult<IReadOnlyList<RolesDto>>> (
            [FromServices] GetRoleHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(cancellationToken));
    }
}

public sealed class GetRoleHandler
{
    private readonly ILogger<GetRoleHandler> _logger;
    private readonly IRolesRepository _rolesRepository;

    public GetRoleHandler(ILogger<GetRoleHandler> logger, IRolesRepository rolesRepository)
    {
        _logger = logger;
        _rolesRepository = rolesRepository;
    }

    public async Task<Result<IReadOnlyList<RolesDto>, Error>> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get all roles");

        return await _rolesRepository.GetAllAsync(cancellationToken);
    }
}
