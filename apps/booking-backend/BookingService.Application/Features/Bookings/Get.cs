using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public record BookingsDto(Guid Id, Guid UserId, Guid BookingObjectId, DateTime StartDateTime, DateTime EndDateTime, int GuestsCount, Comment? Comment, Guid StatusId, DateTime CreatedAt);

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings", async Task<EndpointResult<IReadOnlyList<BookingsDto>>> (
            [FromServices] GetBookingHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(cancellationToken))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class GetBookingHandler
{
    private readonly ILogger<GetBookingHandler> _logger;
    private readonly IBookingRepository _bookingRepository;

    public GetBookingHandler(ILogger<GetBookingHandler> logger, IBookingRepository bookingRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<BookingsDto>, Error>> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get all bookings");

        return await _bookingRepository.GetAllAsync(cancellationToken);
    }
}
