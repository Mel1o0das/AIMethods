using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingObjects;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/booking-objects/{id:guid}", async Task<EndpointResult<BookingObjectsDto>> (
            Guid id,
            [FromServices] GetBookingObjectByIdHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, cancellationToken));
    }
}

public sealed class GetBookingObjectByIdHandler
{
    private readonly ILogger<GetBookingObjectByIdHandler> _logger;
    private readonly IBookingObjectRepository _bookingObjectRepository;

    public GetBookingObjectByIdHandler(ILogger<GetBookingObjectByIdHandler> logger, IBookingObjectRepository bookingObjectRepository)
    {
        _logger = logger;
        _bookingObjectRepository = bookingObjectRepository;
    }

    public async Task<Result<BookingObjectsDto, Error>> Handle(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get booking object by id {Id}", id);

        return await _bookingObjectRepository.GetByIdAsync(id, cancellationToken);
    }
}
