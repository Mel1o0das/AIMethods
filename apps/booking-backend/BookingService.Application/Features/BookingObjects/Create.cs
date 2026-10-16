using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.BookingObjects;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingObjects;

public record CreateBookingObjectRequest(
    string Name,
    string Description,
    decimal Price,
    int Capacity,
    string ImageUrl,
    bool IsActive,
    Guid? ObjectTypeId);

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/booking-objects", async Task<EndpointResult<Guid>> (
            [FromBody] CreateBookingObjectRequest request,
            [FromServices] CreateBookingObjectHandler handler) =>
                await handler.Handle(request))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class CreateBookingObjectHandler
{
    private readonly ILogger<CreateBookingObjectHandler> _logger;
    private readonly IBookingObjectRepository _bookingObjectRepository;

    public CreateBookingObjectHandler(ILogger<CreateBookingObjectHandler> logger, IBookingObjectRepository bookingObjectRepository)
    {
        _logger = logger;
        _bookingObjectRepository = bookingObjectRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreateBookingObjectRequest request)
    {
        var nameResult = Name.Create(request.Name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        var descriptionResult = Description.Create(request.Description);
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        var bookingObject = BookingObject.Create(
            nameResult.Value,
            descriptionResult.Value,
            request.Price,
            request.Capacity,
            request.ImageUrl,
            request.IsActive,
            request.ObjectTypeId);

        var result = await _bookingObjectRepository.AddAsync(bookingObject.Value);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Created booking object {Id}", bookingObject.Value.Id.Value);

        return bookingObject.Value.Id.Value;
    }
}
