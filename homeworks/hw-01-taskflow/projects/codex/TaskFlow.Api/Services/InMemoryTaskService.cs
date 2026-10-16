using System.Collections.Concurrent;
using TaskFlow.Api.Models;
using TaskStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Services;

public sealed class InMemoryTaskService : ITaskService
{
    private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new();

    public IReadOnlyCollection<TaskItem> GetAll() =>
        _tasks.Values.OrderBy(task => task.CreatedAtUtc).ToArray();

    public TaskItem? GetById(Guid id) =>
        _tasks.TryGetValue(id, out var task) ? task : null;

    public TaskItem Create(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
        {
            throw new ArgumentException("Title must contain 1 to 100 characters.", nameof(title));
        }

        var task = new TaskItem(
            Guid.NewGuid(),
            title.Trim(),
            description?.Trim() ?? string.Empty,
            TaskStatus.New,
            DateTime.UtcNow);

        if (!_tasks.TryAdd(task.Id, task))
        {
            throw new InvalidOperationException("Could not create the task.");
        }

        return task;
    }

    public TaskItem? UpdateStatus(Guid id, TaskStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        while (_tasks.TryGetValue(id, out var current))
        {
            var updated = current with { Status = status };
            if (_tasks.TryUpdate(id, updated, current))
            {
                return updated;
            }
        }

        return null;
    }
}
