using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

/// <summary>
/// Thread-safe in-memory repository for tasks.
/// </summary>
public interface ITaskRepository
{
    IReadOnlyCollection<TaskItem> GetAll();

    TaskItem? GetById(Guid id);

    TaskItem Add(TaskItem item);

    void Update(TaskItem item);
}
