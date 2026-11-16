
namespace TasksApi.Api.DTOs.Tasks;

public sealed record PaginatedTasksResponse(
    IReadOnlyList<TaskResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages
);
