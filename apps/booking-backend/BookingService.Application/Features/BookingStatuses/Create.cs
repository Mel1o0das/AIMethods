using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingStatuses;

public record CreateBookingStatusRequest(string Name);

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/booking-statuses", async Task<EndpointResult<Guid>> (
            [FromBody] CreateBookingStatusRequest request,
            [FromServices] CreateBookingStatusHandler handler) =>
                await handler.Handle(request))
            .RequireAuthorization(policy => policy.RequireRole(BookingRules.AdministratorRoleName));
    }
}

public sealed class CreateBookingStatusHandler
{
    private readonly ILogger<CreateBookingStatusHandler> _logger;
    private readonly IBookingStatusRepository _bookingStatusRepository;

    public CreateBookingStatusHandler(ILogger<CreateBookingStatusHandler> logger, IBookingStatusRepository bookingStatusRepository)
    {
        _logger = logger;
        _bookingStatusRepository = bookingStatusRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreateBookingStatusRequest request)
    {
        var nameResult = Name.Create(request.Name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var bookingStatus = BookingStatus.Create(nameResult.Value);

        var result = await _bookingStatusRepository.AddAsync(bookingStatus.Value);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Created booking status {Id}", bookingStatus.Value.Id.Value);

        return bookingStatus.Value.Id.Value;
    }
}
