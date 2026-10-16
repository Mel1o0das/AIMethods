using System.Text.Json.Serialization;

namespace BookingService.Domain.Shared;

public record CustomEnvelope
{
    public object? Result { get; }

    public Error? Error { get; }

    public bool IsError => Error != null;

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private CustomEnvelope(object? result, Error? error)
    {
        Result = result;
        Error = error;
        TimeGenerated = DateTime.UtcNow;
    }

    public static CustomEnvelope Ok(object? result = null) =>
        new(result, null);

    public static CustomEnvelope Fail(Error error) =>
        new(null, error);
}

public record CustomEnvelope<T>
{
    public T? Result { get; }

    public Error? Error { get; }

    public bool IsError => Error != null;

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private CustomEnvelope(T? result, Error? error)
    {
        Result = result;
        Error = error;
        TimeGenerated = DateTime.UtcNow;
    }

    public static CustomEnvelope<T> Ok(T? result = default) =>
        new(result, null);

    public static CustomEnvelope<T> Fail(Error error) =>
        new(default, error);
}
