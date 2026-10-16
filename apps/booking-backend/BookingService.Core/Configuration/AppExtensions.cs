using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Serilog;

namespace BookingService.Core.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            app.Logger.LogError(exception, "Unhandled request error");
            var isBadRequest = exception is BadHttpRequestException;
            context.Response.StatusCode = isBadRequest
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status500InternalServerError;
            var error = isBadRequest
                ? Error.Validation("request.invalid_json", "Request body is invalid")
                : Error.Failure("server.unexpected", "Unexpected server error");
            await context.Response.WriteAsJsonAsync(CustomEnvelope.Fail(error));
        }));

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.ConfigObject.AdditionalItems["persistAuthorization"] = "true";
            });
        }

        app.UseCors();

        string imagesPath = app.Configuration["ImagesPath"] ?? Path.Combine(AppContext.BaseDirectory, "images");
        Directory.CreateDirectory(imagesPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imagesPath),
            RequestPath = "/images"
        });

        app.UseAuthentication();
        app.UseAuthorization();

        RouteGroupBuilder apiGroup = app.MapGroup("/api");
        app.MapEndpoints(apiGroup);
        app.MapHealthChecks("/health");

        return app;
    }
}
