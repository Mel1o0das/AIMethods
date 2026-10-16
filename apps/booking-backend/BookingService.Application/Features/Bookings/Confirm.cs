using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class Confirm : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/bookings/{id:guid}/confirm", async Task<EndpointResult<Guid>> (
            Guid id,
            [FromServices] ConfirmBookingHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, cancellationToken))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class ConfirmBookingHandler
{
    private readonly ILogger<ConfirmBookingHandler> _logger;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public ConfirmBookingHandler(ILogger<ConfirmBookingHandler> logger, IBookingRepository bookingRepository, IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<Guid, Error>> Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var confirmedStatusResult = await _bookingStatusRepository.GetByNameAsync(
            BookingRules.ConfirmedStatusName,
            cancellationToken);
        if (confirmedStatusResult.IsFailure)
            return confirmedStatusResult.Error;

        var cancelledStatusResult = await _bookingStatusRepository.GetByNameAsync(
            BookingRules.CancelledStatusName,
            cancellationToken);
        if (cancelledStatusResult.IsFailure)
            return cancelledStatusResult.Error;

        var result = await _bookingRepository.ConfirmAsync(
            id,
            confirmedStatusResult.Value,
            cancelledStatusResult.Value,
            cancellationToken);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Confirmed booking {Id}", id);

        return result.Value;
    }
}
