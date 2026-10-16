using TaskFlow.Api.Models;
using TaskStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    IReadOnlyCollection<TaskItem> GetAll();
    TaskItem? GetById(Guid id);
    TaskItem Create(string title, string? description);
    TaskItem? UpdateStatus(Guid id, TaskStatus status);
}
