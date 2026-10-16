using System.IO;
using System.Text;
using BookingService.Application;
using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.BookingObjects;
using BookingService.Application.Features.Bookings;
using BookingService.Application.Features.BookingStatuses;
using BookingService.Application.Features.ObjectTypes;
using BookingService.Application.Features.Roles;
using BookingService.Application.Features.Users;
using BookingService.Application.Services;
using BookingService.Domain.Shared;
using BookingService.Infrastructure.Postgres;
using BookingService.Infrastructure.Postgres.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Exceptions;

namespace BookingService.Core.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRolesRepository, RoleRepository>();
        services.AddScoped<IBookingStatusRepository, BookingStatusRepository>();
        services.AddScoped<IObjectTypeRepository, ObjectTypeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingObjectRepository, BookingObjectRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services
            .AddSerilogLogging(configuration)
            .AddOpenApiSpec()
            .AddJwtAuthentication(configuration)
            .AddDataProtectionKeys()
            .AddDbContext(configuration)
            .AddInfrastructure()
            .AddApplication()
            .AddEndpoints(typeof(Application.DependencyInjectionExtensions).Assembly)
            .AddHealthChecks()
            .Services
            .AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                    var origins = configuredOrigins
                        .Append("http://localhost:3000")
                        .Where(origin => !string.IsNullOrWhiteSpace(origin))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
    }

    private static IServiceCollection AddDataProtectionKeys(this IServiceCollection services)
    {
        var keysPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataProtection-Keys");

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .SetApplicationName("BookingService");

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var secretKey = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException("JwtSettings:SecretKey must contain at least 32 characters");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    NameClaimType = "sub",
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(CustomEnvelope.Fail(
                            Error.Authentication("auth.required", "Authentication is required")));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(CustomEnvelope.Fail(
                            Error.Authorization("auth.forbidden", "Insufficient permissions")));
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
    {
        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Booking Service",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "mel1o0das",
                    Email = "mel1o0das@yandex.ru"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите JWT токен (без Bearer)",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document),
                    new List<string>()
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "BookingService"));

        return services;
    }

    private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BookingServiceDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:BookingServiceDb is not configured");

        services.AddScoped<BookingServiceDbContext>(_ => new BookingServiceDbContext(connectionString));

        return services;
    }
}
