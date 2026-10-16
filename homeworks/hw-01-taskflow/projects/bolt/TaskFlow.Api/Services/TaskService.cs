using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

/// <summary>
/// Validates input, enforces business rules, and delegates persistence to the repository.
/// </summary>
public sealed class TaskService : ITaskService
{
    private const int MaxTitleLength = 100;

    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyCollection<TaskItem> GetAll()
    {
        return _repository.GetAll();
    }

    public TaskOperationResult GetById(Guid id)
    {
        var task = _repository.GetById(id);
        return task is null
            ? TaskOperationResult.Fail("Task not found.")
            : TaskOperationResult.Ok(task);
    }

    public TaskOperationResult Create(CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return TaskOperationResult.Fail("Title is required.");
        }

        if (request.Title.Length > MaxTitleLength)
        {
            return TaskOperationResult.Fail($"Title must not exceed {MaxTitleLength} characters.");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description ?? string.Empty,
            Status = TaskItemStatus.New,
            CreatedAtUtc = DateTime.UtcNow
        };

        _repository.Add(task);
        return TaskOperationResult.Ok(task);
    }

    public TaskOperationResult UpdateStatus(Guid id, UpdateTaskStatusRequest request)
    {
        var task = _repository.GetById(id);
        if (task is null)
        {
            return TaskOperationResult.Fail("Task not found.");
        }

        if (!Enum.IsDefined(request.Status))
        {
            return TaskOperationResult.Fail("Invalid status value.");
        }

        task.Status = request.Status;
        _repository.Update(task);
        return TaskOperationResult.Ok(task);
    }
}
