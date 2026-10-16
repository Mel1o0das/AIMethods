using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class Cancel : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/bookings/{id:guid}/cancel", async Task<EndpointResult<Guid>> (
            Guid id,
            ClaimsPrincipal principal,
            [FromServices] CancelBookingHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class CancelBookingHandler
{
    private readonly ILogger<CancelBookingHandler> _logger;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public CancelBookingHandler(ILogger<CancelBookingHandler> logger, IBookingRepository bookingRepository, IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<Guid, Error>> Handle(
        Guid id,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userIdResult = BookingRules.GetUserId(principal);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var statusResult = await _bookingStatusRepository.GetByNameAsync(
            BookingRules.CancelledStatusName,
            cancellationToken);
        if (statusResult.IsFailure)
            return statusResult.Error;

        var result = await _bookingRepository.CancelAsync(
            id,
            userIdResult.Value,
            principal.IsInRole(BookingRules.AdministratorRoleName),
            statusResult.Value,
            cancellationToken);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Cancelled booking {Id}", id);

        return result.Value;
    }
}
