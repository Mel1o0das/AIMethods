using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.ObjectTypes;

public record ObjectTypeDto(Guid Id, Name Name);

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/object-types", async Task<EndpointResult<IReadOnlyList<ObjectTypeDto>>> (
            [FromServices] GetObjectTypeHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(cancellationToken));
    }
}

public sealed class GetObjectTypeHandler
{
    private readonly ILogger<GetObjectTypeHandler> _logger;
    private readonly IObjectTypeRepository _objectTypeRepository;

    public GetObjectTypeHandler(ILogger<GetObjectTypeHandler> logger, IObjectTypeRepository objectTypeRepository)
    {
        _logger = logger;
        _objectTypeRepository = objectTypeRepository;
    }

    public async Task<Result<IReadOnlyList<ObjectTypeDto>, Error>> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get all object types");

        var result = await _objectTypeRepository.GetAllAsync(cancellationToken);

        return result;
    }
}
