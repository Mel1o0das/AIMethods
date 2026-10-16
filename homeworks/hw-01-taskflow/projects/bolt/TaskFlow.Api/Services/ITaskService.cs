using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

/// <summary>
/// Business logic layer for task operations.
/// </summary>
public interface ITaskService
{
    IReadOnlyCollection<TaskItem> GetAll();

    TaskOperationResult GetById(Guid id);

    TaskOperationResult Create(CreateTaskRequest request);

    TaskOperationResult UpdateStatus(Guid id, UpdateTaskStatusRequest request);
}
