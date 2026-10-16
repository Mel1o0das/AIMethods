namespace TaskFlow.Api.Models;

/// <summary>
/// Payload accepted by <c>POST /api/tasks</c>.
/// </summary>
public sealed class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Payload accepted by <c>PATCH /api/tasks/{id}/status</c>.
/// </summary>
public sealed class UpdateTaskStatusRequest
{
    public TaskItemStatus Status { get; set; }
}

/// <summary>
/// Standard error envelope returned for 4xx/5xx responses.
/// </summary>
public sealed class ErrorDetail
{
    public string Detail { get; set; } = string.Empty;
}

/// <summary>
/// Result wrapper for service operations that may fail with a known error type.
/// </summary>
public sealed class TaskOperationResult
{
    public TaskItem? Task { get; init; }

    public string? Error { get; init; }

    public bool IsSuccess => Error is null;

    public static TaskOperationResult Ok(TaskItem task) => new() { Task = task };

    public static TaskOperationResult Fail(string error) => new() { Error = error };
}
