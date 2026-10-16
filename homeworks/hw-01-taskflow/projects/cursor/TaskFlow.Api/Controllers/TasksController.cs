using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<TaskItem>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<TaskItem>> GetAll()
    {
        return Ok(_taskService.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TaskItem> GetById(Guid id)
    {
        var task = _taskService.GetById(id);
        if (task is null)
        {
            return NotFound(new { message = $"Task '{id}' was not found." });
        }

        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TaskItem> Create([FromBody] CreateTaskRequest request)
    {
        try
        {
            var created = _taskService.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TaskItem> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            var updated = _taskService.UpdateStatus(id, request.Status);
            if (updated is null)
            {
                return NotFound(new { message = $"Task '{id}' was not found." });
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
