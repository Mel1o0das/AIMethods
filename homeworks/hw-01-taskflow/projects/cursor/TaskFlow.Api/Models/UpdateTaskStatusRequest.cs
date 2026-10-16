using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public class UpdateTaskStatusRequest
{
    [Required(ErrorMessage = "Status is required.")]
    public TaskItemStatus Status { get; set; }
}
