using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings/{id:guid}", async Task<EndpointResult<BookingsDto>> (
            Guid id,
            ClaimsPrincipal principal,
            [FromServices] GetBookingByIdHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class GetBookingByIdHandler
{
    private readonly ILogger<GetBookingByIdHandler> _logger;
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdHandler(
        ILogger<GetBookingByIdHandler> logger,
        IBookingRepository bookingRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingsDto, Error>> Handle(
        Guid id,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userIdString = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId))
            return Error.Authentication("auth.invalid_token", "Invalid token");

        var bookingResult = await _bookingRepository.GetByIdAsync(id, cancellationToken);
        if (bookingResult.IsFailure)
            return bookingResult.Error;

        var booking = bookingResult.Value;
        if (booking.UserId != userId && !IsAdminUser(principal))
            return Error.Authorization("booking.access_denied", "Access to the booking is forbidden");

        _logger.LogInformation("Get booking {Id} for user {UserId}", id, userId);

        return booking;
    }

    private static bool IsAdminUser(ClaimsPrincipal principal)
    {
        var roleName = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return roleName.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || roleName.Contains("админ", StringComparison.OrdinalIgnoreCase);
    }
}
