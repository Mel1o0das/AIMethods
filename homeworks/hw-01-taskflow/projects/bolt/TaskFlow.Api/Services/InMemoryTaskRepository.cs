using System.Collections.Concurrent;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

/// <summary>
/// Thread-safe in-memory repository backed by a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// All read/write operations are atomic.
/// </summary>
public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<Guid, TaskItem> _store = new();

    public IReadOnlyCollection<TaskItem> GetAll()
    {
        return _store.Values.ToArray();
    }

    public TaskItem? GetById(Guid id)
    {
        return _store.TryGetValue(id, out var item) ? item : null;
    }

    public TaskItem Add(TaskItem item)
    {
        _store[item.Id] = item;
        return item;
    }

    public void Update(TaskItem item)
    {
        _store[item.Id] = item;
    }
}
