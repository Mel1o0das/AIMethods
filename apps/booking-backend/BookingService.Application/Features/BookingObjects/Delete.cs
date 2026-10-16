using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingObjects;

public class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/booking-objects/{id:guid}", async Task<EndpointResult<Guid>> (
            Guid id,
            [FromServices] DeleteBookingObjectHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, cancellationToken))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class DeleteBookingObjectHandler
{
    private readonly ILogger<DeleteBookingObjectHandler> _logger;
    private readonly IBookingObjectRepository _bookingObjectRepository;

    public DeleteBookingObjectHandler(ILogger<DeleteBookingObjectHandler> logger, IBookingObjectRepository bookingObjectRepository)
    {
        _logger = logger;
        _bookingObjectRepository = bookingObjectRepository;
    }

    public async Task<Result<Guid, Error>> Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _bookingObjectRepository.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Deleted booking object {Id}", id);

        return result.Value;
    }
}
