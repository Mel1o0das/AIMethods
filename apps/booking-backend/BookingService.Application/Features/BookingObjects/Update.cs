using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingObjects;

public record UpdateBookingObjectRequest(
    string Name,
    string Description,
    decimal Price,
    int Capacity,
    string? ImageUrl,
    bool IsActive,
    Guid? ObjectTypeId);

public class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/booking-objects/{id:guid}", async Task<EndpointResult<Guid>> (
            Guid id,
            [FromBody] UpdateBookingObjectRequest request,
            [FromServices] UpdateBookingObjectHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, request, cancellationToken))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class UpdateBookingObjectHandler
{
    private readonly ILogger<UpdateBookingObjectHandler> _logger;
    private readonly IBookingObjectRepository _bookingObjectRepository;

    public UpdateBookingObjectHandler(ILogger<UpdateBookingObjectHandler> logger, IBookingObjectRepository bookingObjectRepository)
    {
        _logger = logger;
        _bookingObjectRepository = bookingObjectRepository;
    }

    public async Task<Result<Guid, Error>> Handle(Guid id, UpdateBookingObjectRequest request, CancellationToken cancellationToken = default)
    {
        var nameResult = Name.Create(request.Name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        var descriptionResult = Description.Create(request.Description);
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        var result = await _bookingObjectRepository.UpdateAsync(
            id,
            nameResult.Value,
            descriptionResult.Value,
            request.Price,
            request.Capacity,
            request.ImageUrl,
            request.IsActive,
            request.ObjectTypeId,
            cancellationToken);

        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Updated booking object {Id}", id);

        return result.Value;
    }
}
