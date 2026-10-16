using System.Collections.Concurrent;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Storage;

public class InMemoryTaskStore : ITaskStore
{
    private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new();

    public IReadOnlyCollection<TaskItem> GetAll()
    {
        return _tasks.Values
            .Select(Clone)
            .OrderByDescending(task => task.CreatedAtUtc)
            .ToArray();
    }

    public TaskItem? GetById(Guid id)
    {
        return _tasks.TryGetValue(id, out var task) ? Clone(task) : null;
    }

    public TaskItem Add(TaskItem item)
    {
        var stored = Clone(item);
        if (!_tasks.TryAdd(stored.Id, stored))
        {
            throw new InvalidOperationException($"Task with id '{stored.Id}' already exists.");
        }

        return Clone(stored);
    }

    public TaskItem? UpdateStatus(Guid id, TaskItemStatus status)
    {
        if (!_tasks.TryGetValue(id, out var existing))
        {
            return null;
        }

        var updated = Clone(existing);
        updated.Status = status;

        if (!_tasks.TryUpdate(id, updated, existing))
        {
            return UpdateStatus(id, status);
        }

        return Clone(updated);
    }

    private static TaskItem Clone(TaskItem item)
    {
        return new TaskItem
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Status = item.Status,
            CreatedAtUtc = item.CreatedAtUtc
        };
    }
}
