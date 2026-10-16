using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public record OccupiedSlotDto(DateTime StartDateTime, DateTime EndDateTime);

public class GetAvailability : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/availability", async Task<EndpointResult<IReadOnlyList<OccupiedSlotDto>>> (
            [FromQuery] Guid objectId,
            [FromQuery] DateOnly date,
            [FromServices] GetAvailabilityHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(objectId, date, cancellationToken));
    }
}

public sealed class GetAvailabilityHandler
{
    private readonly ILogger<GetAvailabilityHandler> _logger;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public GetAvailabilityHandler(
        ILogger<GetAvailabilityHandler> logger,
        IBookingRepository bookingRepository,
        IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<IReadOnlyList<OccupiedSlotDto>, Error>> Handle(
        Guid objectId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var cancelledStatusResult = await _bookingStatusRepository.GetByNameAsync("Отменено", cancellationToken);
        if (cancelledStatusResult.IsFailure)
            return cancelledStatusResult.Error;

        var slots = await _bookingRepository.GetOccupiedSlotsAsync(
            objectId,
            date,
            cancelledStatusResult.Value,
            cancellationToken);

        _logger.LogInformation("Get occupied slots for object {ObjectId} on {Date}", objectId, date);

        return slots;
    }
}
