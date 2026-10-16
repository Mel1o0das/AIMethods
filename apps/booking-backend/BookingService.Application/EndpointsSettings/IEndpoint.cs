using Microsoft.AspNetCore.Routing;

namespace BookingService.Application.EndpointsSettings;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
