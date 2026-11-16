using System.ComponentModel.DataAnnotations;
using TasksApi.Api.Domain.Enums;

namespace TasksApi.Api.DTOs.Tasks;

public sealed class CreateTaskRequest
{
    [Required]
    [StringLength(120)]
    public string Title { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [EnumDataType(typeof(TaskState))]
    public TaskState? Status { get; init; }

    [Required]
    [StringLength(100)]
    public string Responsible { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset? DueDate { get; init; }
}