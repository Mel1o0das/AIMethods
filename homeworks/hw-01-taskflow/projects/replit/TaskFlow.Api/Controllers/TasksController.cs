using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<TaskItem>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<TaskItem>> GetAll()
    {
        return Ok(taskService.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TaskItem> GetById(Guid id)
    {
        var task = taskService.GetById(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [ProducesResponseType<TaskItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TaskItem> Create(CreateTaskRequest request)
    {
        var task = taskService.Create(request.Title!, request.Description);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<TaskItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TaskItem> UpdateStatus(Guid id, UpdateTaskStatusRequest request)
    {
        if (!Enum.IsDefined(request.Status!.Value))
        {
            return BadRequest(new
            {
                errors = new { Status = new[] { "Status must be New, InProgress, or Completed." } }
            });
        }

        var task = taskService.UpdateStatus(id, request.Status.Value);
        return task is null ? NotFound() : Ok(task);
    }
}
