using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.ObjectTypes;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.ObjectTypes;

public record CreateObjectTypeRequest(string Name);

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/object-types", async Task<EndpointResult<Guid>> (
            [FromBody] CreateObjectTypeRequest request,
            [FromServices] CreateObjectTypeHandler handler) =>
                await handler.Handle(request))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class CreateObjectTypeHandler
{
    private readonly ILogger<CreateObjectTypeHandler> _logger;
    private readonly IObjectTypeRepository _objectTypeRepository;

    public CreateObjectTypeHandler(ILogger<CreateObjectTypeHandler> logger, IObjectTypeRepository objectTypeRepository)
    {
        _logger = logger;
        _objectTypeRepository = objectTypeRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreateObjectTypeRequest request)
    {
        var nameResult = Name.Create(request.Name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        var objectType = ObjectType.Create(nameResult.Value);

        var result = await _objectTypeRepository.AddAsync(objectType.Value);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Created object type {Id}", objectType.Value.Id.Value);

        return objectType.Value.Id.Value;
    }
}
