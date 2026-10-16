using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public sealed class UpdateTaskStatusRequest
{
    [Required]
    public TaskItemStatus? Status { get; init; }
}
