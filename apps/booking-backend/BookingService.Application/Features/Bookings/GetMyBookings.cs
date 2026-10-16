using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class GetMyBookings : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings/my/{id:guid}", async Task<EndpointResult<IReadOnlyList<BookingsDto>>> (
            Guid id,
            ClaimsPrincipal principal,
            [FromServices] GetMyBookingsHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class GetMyBookingsHandler
{
    private readonly ILogger<GetMyBookingsHandler> _logger;
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsHandler(ILogger<GetMyBookingsHandler> logger, IBookingRepository bookingRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<BookingsDto>, Error>> Handle(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var userIdResult = BookingRules.GetUserId(principal);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var isAdministrator = principal.IsInRole(BookingRules.AdministratorRoleName);
        if (!isAdministrator && id != userIdResult.Value)
            return Error.Authorization("booking.read.forbidden", "Cannot read another user's bookings");

        _logger.LogInformation("Get my bookings");

        return await _bookingRepository.GetMyAsync(id, cancellationToken);
    }
}
