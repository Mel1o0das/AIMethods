using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskFlow.Api.Models;

namespace TaskFlow.Tests;

public sealed class TasksApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public TasksApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_WithValidTitle_ReturnsCreatedTask()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Prepare release",
            description = "Write release notes"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);
        Assert.NotNull(task);
        Assert.Equal("Prepare release", task.Title);
        Assert.Equal("Write release notes", task.Description);
        Assert.Equal(TaskItemStatus.New, task.Status);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.NotEqual(default, task.CreatedAtUtc);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = "",
            description = "Title is missing"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTaskStatus_WithExistingTask_ReturnsUpdatedTask()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Complete API"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);
        Assert.NotNull(createdTask);

        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/tasks/{createdTask.Id}/status",
            new { status = "Completed" });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);
        Assert.NotNull(updatedTask);
        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.Equal(TaskItemStatus.Completed, updatedTask.Status);
    }

    [Fact]
    public async Task CreateTask_WithTitleOver100Characters_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = new string('x', 101)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTask_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerDocument_IsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.StartsWith("3.", document.RootElement.GetProperty("openapi").GetString());
    }
}
