namespace TaskFlow.Api.Models;

/// <summary>
/// Represents the current state of a task in its lifecycle.
/// </summary>
public enum TaskItemStatus
{
    New = 0,
    InProgress = 1,
    Completed = 2
}
