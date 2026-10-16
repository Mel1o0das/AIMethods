using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.BookingObjects;

public record BookingObjectsDto(Guid Id, Name Name, Description Description, decimal Price, int Capacity, string? ImageUrl, bool IsActive, DateTime CreatedAt, Guid? ObjectTypeId);

public record BookingObjectsPagedQuery(
    int Page,
    int PageSize,
    string? Search,
    string? SortBy,
    string? SortDir,
    bool IncludeInactive,
    Guid? TypeId,
    decimal? MaxPrice,
    int? MinCapacity);

public record BookingObjectsPagedResult(
    IReadOnlyList<BookingObjectsDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/booking-objects",
            async Task<EndpointResult<BookingObjectsPagedResult>> (
                [FromServices] GetBookingObjectHandler handler,
                HttpContext httpContext,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? sortDir = null,
                [FromQuery] Guid? typeId = null,
                [FromQuery] decimal? maxPrice = null,
                [FromQuery] int? minCapacity = null,
                CancellationToken cancellationToken = default) =>
            await handler.Handle(
                new BookingObjectsPagedQuery(
                    page,
                    pageSize,
                    search,
                    sortBy,
                    sortDir,
                    IsAdminUser(httpContext.User),
                    typeId,
                    maxPrice,
                    minCapacity),
                cancellationToken));
    }

    private static bool IsAdminUser(ClaimsPrincipal user)
    {
        var roleName = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return roleName.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || roleName.Contains("админ", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class GetBookingObjectHandler
{
    private readonly ILogger<GetBookingObjectHandler> _logger;
    private readonly IBookingObjectRepository _bookingObjectRepository;

    public GetBookingObjectHandler(ILogger<GetBookingObjectHandler> logger, IBookingObjectRepository bookingObjectRepository)
    {
        _logger = logger;
        _bookingObjectRepository = bookingObjectRepository;
    }

    public async Task<Result<BookingObjectsPagedResult, Error>> Handle(BookingObjectsPagedQuery query, CancellationToken cancellationToken = default)
    {
        var validPage = Math.Max(1, query.Page);
        var validPageSize = Math.Clamp(query.PageSize, 1, 100);
        var validatedQuery = query with { Page = validPage, PageSize = validPageSize };

        _logger.LogInformation("Get booking objects page {Page} size {PageSize}", validatedQuery.Page, validatedQuery.PageSize);

        return await _bookingObjectRepository.GetPagedAsync(validatedQuery, cancellationToken);
    }
}
