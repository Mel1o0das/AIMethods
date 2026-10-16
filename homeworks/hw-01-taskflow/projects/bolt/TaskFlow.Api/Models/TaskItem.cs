namespace TaskFlow.Api.Models;

/// <summary>
/// A single task tracked by the TaskFlow application.
/// </summary>
public sealed class TaskItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TaskItemStatus Status { get; set; } = TaskItemStatus.New;

    public DateTime CreatedAtUtc { get; set; }
}
