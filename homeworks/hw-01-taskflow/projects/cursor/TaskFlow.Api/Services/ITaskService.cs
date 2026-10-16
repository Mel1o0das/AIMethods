using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    IReadOnlyCollection<TaskItem> GetAll();
    TaskItem? GetById(Guid id);
    TaskItem Create(CreateTaskRequest request);
    TaskItem? UpdateStatus(Guid id, TaskItemStatus status);
}
