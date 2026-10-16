using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(1, ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title must be at most 100 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}
