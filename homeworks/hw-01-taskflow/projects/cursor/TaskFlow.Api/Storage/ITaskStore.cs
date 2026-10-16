using TaskFlow.Api.Models;

namespace TaskFlow.Api.Storage;

public interface ITaskStore
{
    IReadOnlyCollection<TaskItem> GetAll();
    TaskItem? GetById(Guid id);
    TaskItem Add(TaskItem item);
    TaskItem? UpdateStatus(Guid id, TaskItemStatus status);
}
