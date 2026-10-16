using System.Collections.Concurrent;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public sealed class InMemoryTaskService : ITaskService
{
    private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new();

    public IReadOnlyCollection<TaskItem> GetAll() => _tasks.Values.ToArray();

    public TaskItem? GetById(Guid id) =>
        _tasks.TryGetValue(id, out var task) ? task : null;

    public TaskItem Create(string title, string? description)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description ?? string.Empty,
            Status = TaskItemStatus.New,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (!_tasks.TryAdd(task.Id, task))
        {
            throw new InvalidOperationException("A task with the generated ID already exists.");
        }

        return task;
    }

    public TaskItem? UpdateStatus(Guid id, TaskItemStatus status)
    {
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
