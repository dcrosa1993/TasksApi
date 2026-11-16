
using System.ComponentModel.DataAnnotations;
using TasksApi.Api.Domain.Enums;

namespace TasksApi.Api.DTOs.Tasks;

public sealed class GetTasksRequest
{
    [EnumDataType(typeof(TaskState))]
    public TaskState? Status { get; init; }

    [StringLength(100)]
    public string? Responsible { get; init; }

    [Range(1, 1_000_000)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}
