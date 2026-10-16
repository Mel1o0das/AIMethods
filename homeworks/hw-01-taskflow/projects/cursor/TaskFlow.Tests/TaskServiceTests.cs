using TaskFlow.Api.Models;
using TaskFlow.Api.Services;
using TaskFlow.Api.Storage;

namespace TaskFlow.Tests;

public class TaskServiceTests
{
    private static TaskService CreateSut() => new(new InMemoryTaskStore());

    [Fact]
    public void Create_WithValidTitle_CreatesTask()
    {
        var sut = CreateSut();
        var request = new CreateTaskRequest
        {
            Title = "Prepare TaskFlow demo",
            Description = "Create REST API endpoints"
        };

        var created = sut.Create(request);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Prepare TaskFlow demo", created.Title);
        Assert.Equal("Create REST API endpoints", created.Description);
        Assert.Equal(TaskItemStatus.New, created.Status);
        Assert.True(created.CreatedAtUtc <= DateTime.UtcNow);
        Assert.True(created.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ThrowsArgumentException(string? title)
    {
        var sut = CreateSut();
        var request = new CreateTaskRequest { Title = title! };

        var exception = Assert.Throws<ArgumentException>(() => sut.Create(request));

        Assert.Contains("Title is required", exception.Message);
    }

    [Fact]
    public void UpdateStatus_ForExistingTask_ChangesStatus()
    {
        var sut = CreateSut();
        var created = sut.Create(new CreateTaskRequest { Title = "Ship API" });

        var updated = sut.UpdateStatus(created.Id, TaskItemStatus.InProgress);

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(TaskItemStatus.InProgress, updated.Status);
        Assert.Equal(TaskItemStatus.InProgress, sut.GetById(created.Id)!.Status);
    }
}
