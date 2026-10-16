using BookingService.Application.Features.Auth;
using BookingService.Application.Features.BookingObjects;
using BookingService.Application.Features.Bookings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Application.Features.ObjectTypes;
using BookingService.Application.Features.Roles;
using BookingService.Application.Features.Users;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginHandler>();
        services.AddScoped<GetMeHandler>();

        services.AddScoped<GetRoleHandler>();
        services.AddScoped<CreateRoleHandler>();

        services.AddScoped<GetUserHandler>();
        services.AddScoped<CreateUserHandler>();

        services.AddScoped<GetBookingHandler>();
        services.AddScoped<GetBookingByIdHandler>();
        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<ConfirmBookingHandler>();
        services.AddScoped<CancelBookingHandler>();
        services.AddScoped<GetMyBookingsHandler>();
        services.AddScoped<GetAvailabilityHandler>();
        services.AddScoped<GetAvailabilityRangeHandler>();

        services.AddScoped<GetBookingObjectHandler>();
        services.AddScoped<GetBookingObjectByIdHandler>();
        services.AddScoped<GetActiveBookingObjectCountHandler>();
        services.AddScoped<CreateBookingObjectHandler>();
        services.AddScoped<UpdateBookingObjectHandler>();
        services.AddScoped<DeleteBookingObjectHandler>();

        services.AddScoped<GetBookingStatusHandler>();
        services.AddScoped<CreateBookingStatusHandler>();

        services.AddScoped<GetObjectTypeHandler>();
        services.AddScoped<CreateObjectTypeHandler>();

        return services;
    }
}
