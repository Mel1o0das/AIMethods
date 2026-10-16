using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskFlow.Api.Models;

namespace TaskFlow.Tests;

/// <summary>
/// Integration tests that exercise the API end-to-end through the HTTP pipeline.
/// </summary>
public class TaskFlowApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TaskFlowApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Test 1: successful task creation
    [Fact]
    public async Task CreateTask_ValidTitle_ReturnsCreated()
    {
        var request = new CreateTaskRequest
        {
            Title = "Write integration tests",
            Description = "Cover the core endpoints"
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var task = await response.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);
        Assert.NotNull(task);
        Assert.Equal("Write integration tests", task!.Title);
        Assert.Equal("Cover the core endpoints", task.Description);
        Assert.Equal(TaskItemStatus.New, task.Status);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.True((DateTime.UtcNow - task.CreatedAtUtc).TotalSeconds < 5);
    }

    // Test 2: error on empty title
    [Fact]
    public async Task CreateTask_EmptyTitle_ReturnsBadRequest()
    {
        var request = new CreateTaskRequest
        {
            Title = "",
            Description = "Should not be saved"
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Test 3: successful status change
    [Fact]
    public async Task UpdateStatus_ExistingTask_ReturnsOkWithUpdatedStatus()
    {
        // Arrange — create a task first
        var createRequest = new CreateTaskRequest
        {
            Title = "Review pull request",
            Description = "Code review for feature branch"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);

        Assert.NotNull(created);

        // Act — update the status to InProgress
        var patch = new UpdateTaskStatusRequest { Status = TaskItemStatus.InProgress };
        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}/status",
            patch);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(TaskItemStatus.InProgress, updated!.Status);
        Assert.Equal(created.Id, updated.Id);
    }
}
