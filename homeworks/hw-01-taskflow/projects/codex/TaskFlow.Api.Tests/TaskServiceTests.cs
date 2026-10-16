using TaskFlow.Api.Services;
using TaskStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Tests;

public sealed class TaskServiceTests
{
    [Fact]
    public void Create_WithValidTitle_ReturnsNewTask()
    {
        var service = new InMemoryTaskService();

        var task = service.Create("Write tests", "Check the API");

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Write tests", task.Title);
        Assert.Equal("Check the API", task.Description);
        Assert.Equal(TaskStatus.New, task.Status);
        Assert.Equal(DateTimeKind.Utc, task.CreatedAtUtc.Kind);
        Assert.Equal(task, service.GetById(task.Id));
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsValidationError()
    {
        var service = new InMemoryTaskService();

        Assert.Throws<ArgumentException>(() => service.Create("   ", null));
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void UpdateStatus_ForExistingTask_PersistsNewStatus()
    {
        var service = new InMemoryTaskService();
        var task = service.Create("Implement API", null);

        var updated = service.UpdateStatus(task.Id, TaskStatus.Completed);

        Assert.NotNull(updated);
        Assert.Equal(TaskStatus.Completed, updated.Status);
        Assert.Equal(TaskStatus.Completed, service.GetById(task.Id)?.Status);
        Assert.Equal(task.CreatedAtUtc, updated.CreatedAtUtc);
    }
}
