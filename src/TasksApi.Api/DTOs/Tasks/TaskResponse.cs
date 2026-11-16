using TasksApi.Api.Domain.Enums;

namespace TasksApi.Api.DTOs.Tasks;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskState Status,
    string Responsible,
    DateTimeOffset DueDate
);