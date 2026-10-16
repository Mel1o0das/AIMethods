using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Contracts;

public sealed class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title must not exceed 100 characters.")]
    public string? Title { get; init; }

    public string? Description { get; init; }
}
