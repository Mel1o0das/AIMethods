using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Contracts;

public sealed class UpdateTaskStatusRequest
{
    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(TaskStatus), ErrorMessage = "Status is invalid.")]
    public TaskStatus? Status { get; init; }
}
