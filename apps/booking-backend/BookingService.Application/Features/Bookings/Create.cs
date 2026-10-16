using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.BookingObjects;
using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public record CreateBookingRequest(
    Guid UserId,
    Guid BookingObjectId,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int GuestsCount,
    string? Comment,
    Guid StatusId);

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/bookings", async Task<EndpointResult<Guid>> (
            [FromBody] CreateBookingRequest request,
            ClaimsPrincipal principal,
            [FromServices] CreateBookingHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(request, principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class CreateBookingHandler
{
    private readonly ILogger<CreateBookingHandler> _logger;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public CreateBookingHandler(
        ILogger<CreateBookingHandler> logger,
        IBookingRepository bookingRepository,
        IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateBookingRequest request,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userIdResult = BookingRules.GetUserId(principal);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var pendingStatusResult = await _bookingStatusRepository.GetByNameAsync(
            BookingRules.PendingStatusName,
            cancellationToken);
        if (pendingStatusResult.IsFailure)
            return pendingStatusResult.Error;

        var cancelledStatusResult = await _bookingStatusRepository.GetByNameAsync(
            BookingRules.CancelledStatusName,
            cancellationToken);
        if (cancelledStatusResult.IsFailure)
            return cancelledStatusResult.Error;

        Comment? comment = null;
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            var commentResult = Comment.Create(request.Comment);
            if (commentResult.IsFailure)
                return commentResult.Error;
            comment = commentResult.Value;
        }

        var booking = Booking.Create(
            UserId.Create(userIdResult.Value),
            BookingObjectId.Create(request.BookingObjectId),
            request.StartDateTime,
            request.EndDateTime,
            request.GuestsCount,
            comment,
            pendingStatusResult.Value);

        if (booking.IsFailure)
            return booking.Error;

        var result = await _bookingRepository.AddIfAvailableAsync(
            booking.Value,
            cancelledStatusResult.Value,
            cancellationToken);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Created booking {Id}", booking.Value.Id.Value);

        return booking.Value.Id.Value;
    }
}
