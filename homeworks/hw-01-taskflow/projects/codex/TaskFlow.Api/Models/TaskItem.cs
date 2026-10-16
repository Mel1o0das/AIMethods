namespace TaskFlow.Api.Models;

public sealed record TaskItem(
    Guid Id,
    string Title,
    string Description,
    TaskStatus Status,
    DateTime CreatedAtUtc);
