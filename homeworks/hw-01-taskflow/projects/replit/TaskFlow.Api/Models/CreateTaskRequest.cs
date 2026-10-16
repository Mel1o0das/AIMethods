using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public sealed class CreateTaskRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? Title { get; init; }

    public string? Description { get; init; }
}
