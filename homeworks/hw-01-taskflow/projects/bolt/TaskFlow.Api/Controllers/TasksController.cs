using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// CRUD endpoints for TaskFlow tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns every task.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var tasks = _service.GetAll();
        return Ok(tasks);
    }

    /// <summary>
    /// Returns a single task by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var result = _service.GetById(id);
        return result.IsSuccess
            ? Ok(result.Task)
            : NotFound(new ErrorDetail { Detail = result.Error! });
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateTaskRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorDetail { Detail = "Request body is required." });
        }

        var result = _service.Create(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Task!.Id }, result.Task)
            : BadRequest(new ErrorDetail { Detail = result.Error! });
    }

    /// <summary>
    /// Updates the status of a task.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status404NotFound)]
    public IActionResult UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorDetail { Detail = "Request body is required." });
        }

        var result = _service.UpdateStatus(id, request);
        if (result.IsSuccess)
        {
            return Ok(result.Task);
        }

        return result.Error == "Task not found."
            ? NotFound(new ErrorDetail { Detail = result.Error! })
            : BadRequest(new ErrorDetail { Detail = result.Error! });
    }
}
