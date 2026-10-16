using TaskFlow.Api.Models;
using TaskFlow.Api.Storage;

namespace TaskFlow.Api.Services;

public class TaskService : ITaskService
{
    private const int MaxTitleLength = 100;
    private readonly ITaskStore _store;

    public TaskService(ITaskStore store)
    {
        _store = store;
    }

    public IReadOnlyCollection<TaskItem> GetAll() => _store.GetAll();

    public TaskItem? GetById(Guid id) => _store.GetById(id);

    public TaskItem Create(CreateTaskRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTitle(request.Title);

        var item = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = TaskItemStatus.New,
            CreatedAtUtc = DateTime.UtcNow
        };

        return _store.Add(item);
    }

    public TaskItem? UpdateStatus(Guid id, TaskItemStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Unknown task status.");
        }

        return _store.UpdateStatus(id, status);
    }

    private static void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (title.Trim().Length > MaxTitleLength)
        {
            throw new ArgumentException($"Title must be at most {MaxTitleLength} characters.", nameof(title));
        }
    }
}
