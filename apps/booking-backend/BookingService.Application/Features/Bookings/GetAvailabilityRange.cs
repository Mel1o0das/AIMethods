using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class GetAvailabilityRange : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/availability/range", async Task<EndpointResult<IReadOnlyList<OccupiedSlotDto>>> (
            [FromQuery] Guid objectId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromServices] GetAvailabilityRangeHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(objectId, from, to, cancellationToken));
    }
}

public sealed class GetAvailabilityRangeHandler
{
    private readonly ILogger<GetAvailabilityRangeHandler> _logger;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public GetAvailabilityRangeHandler(
        ILogger<GetAvailabilityRangeHandler> logger,
        IBookingRepository bookingRepository,
        IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<IReadOnlyList<OccupiedSlotDto>, Error>> Handle(
        Guid objectId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var cancelledStatusResult = await _bookingStatusRepository.GetByNameAsync("Отменено", cancellationToken);
        if (cancelledStatusResult.IsFailure)
            return cancelledStatusResult.Error;

        var slots = await _bookingRepository.GetOccupiedSlotsForRangeAsync(
            objectId,
            from,
            to,
            cancelledStatusResult.Value,
            cancellationToken);

        _logger.LogInformation("Get occupied slots range for object {ObjectId} from {From} to {To}", objectId, from, to);

        return slots;
    }
}
