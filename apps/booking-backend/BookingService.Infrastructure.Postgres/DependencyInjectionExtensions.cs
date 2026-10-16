using BookingService.Application.Features.BookingObjects;
using BookingService.Application.Features.Bookings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Application.Features.Roles;
using BookingService.Application.Features.Users;
using BookingService.Application.Services;
using BookingService.Infrastructure.Postgres.Repositories;
using BookingService.Infrastructure.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Postgres
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IRolesRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingObjectRepository, BookingObjectRepository>();
            services.AddScoped<IBookingStatusRepository, BookingStatusRepository>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
