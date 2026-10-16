using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingStatuses;

public record BookingStatusesDto(Guid Id, Name Name);

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/booking-statuses", async Task<EndpointResult<IReadOnlyList<BookingStatusesDto>>> (
            [FromServices] GetBookingStatusHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(cancellationToken));
    }
}

public sealed class GetBookingStatusHandler
{
    private readonly ILogger<GetBookingStatusHandler> _logger;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public GetBookingStatusHandler(ILogger<GetBookingStatusHandler> logger, IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<IReadOnlyList<BookingStatusesDto>, Error>> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get all booking statuses");

        return await _bookingStatusRepository.GetAllAsync(cancellationToken);
    }
}
