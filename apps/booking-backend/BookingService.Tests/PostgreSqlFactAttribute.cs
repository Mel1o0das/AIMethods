using Npgsql;
using Xunit;

namespace BookingService.Tests;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (!PostgreSqlTestDatabase.IsConfigured)
        {
            Skip = $"Set {PostgreSqlTestDatabase.ConnectionStringEnvironmentVariable} to a dedicated PostgreSQL test database";
        }
    }
}

internal static class PostgreSqlTestDatabase
{
    public const string ConnectionStringEnvironmentVariable = "BOOKING_SERVICE_TEST_CONNECTION_STRING";

    private static readonly SemaphoreSlim _migrationLock = new(1, 1);
    private static bool _isMigrated;

    public static string? ConnectionString => Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

    public static bool IsConfigured
    {
        get
        {
            string? connectionString = ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return builder.Database?.Contains("test", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }

    public static BookingServiceDbContext CreateContext()
    {
        string connectionString = ConnectionString
            ?? throw new InvalidOperationException("PostgreSQL test connection string is not configured");

        return new BookingServiceDbContext(connectionString);
    }

    public static async Task EnsureMigratedAsync()
    {
        await _migrationLock.WaitAsync();
        try
        {
            if (_isMigrated)
                return;

            await using BookingServiceDbContext context = CreateContext();
            await context.Database.MigrateAsync();
            _isMigrated = true;
        }
        finally
        {
            _migrationLock.Release();
        }
    }
}
