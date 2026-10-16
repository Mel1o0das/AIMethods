using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public sealed record TaskItem
{
    public Guid Id { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Title { get; init; }

    public string Description { get; init; } = string.Empty;

    public TaskItemStatus Status { get; init; } = TaskItemStatus.New;

    public DateTime CreatedAtUtc { get; init; }
}
