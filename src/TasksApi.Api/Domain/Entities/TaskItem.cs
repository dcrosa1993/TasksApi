using TasksApi.Api.Domain.Enums;

namespace TasksApi.Api.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskState Status { get; set; } = TaskState.Pending;

    public string Responsible { get; set; } = string.Empty;

    public DateTimeOffset DueDate { get; set; }
}