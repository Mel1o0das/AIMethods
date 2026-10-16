using System.Globalization;
using BookingService.Core.Configuration;
using BookingService.Infrastructure.Postgres;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddConfiguration(builder.Configuration);

    WebApplication app = builder.Build();

    await app.Services.InitializeDatabaseAsync(
        app.Configuration.GetValue<bool>("SeedData:Enabled"));

    app.Configure();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
