using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TasksApi.Api.Domain.Enums;
using TasksApi.Api.DTOs.Tasks;
using TasksApi.Tests.Infrastructure;

namespace TasksApi.Tests.Controllers;

public class TasksControllerTests
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

    private static readonly DateTimeOffset DueDate =
        new(2026, 11, 20, 18, 0, 0, TimeSpan.FromHours(-4));

    [Fact]
    public async Task CreateTask_WithoutStatus_ReturnsCreatedAndPending()
    {
        using var api = await TestApi.CreateAsync();

        var payload = new
        {
            title = "Integration test task",
            description = "Testing task creation",
            responsible = "Denismay",
            dueDate = DueDate
        };

        var response = await api.Client.PostAsJsonAsync(
            "/api/tasks",
            payload,
            JsonOptions
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TaskResponse>(
            JsonOptions
        );

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(TaskState.Pending, created.Status);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(created.Id.ToString(), response.Headers.Location.ToString());
    }

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsTask()
    {
        using var api = await TestApi.CreateAsync();

        var created = await CreateTaskAsync(
            api.Client,
            "Task to retrieve",
            TaskState.InProgress,
            "Denismay"
        );

        var response = await api.Client.GetAsync(
            $"/api/tasks/{created.Id}"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(
            JsonOptions
        );

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Task to retrieve", result.Title);
        Assert.Equal(TaskState.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateTask_ExistingTask_UpdatesSuccessfully()
    {
        using var api = await TestApi.CreateAsync();

        var created = await CreateTaskAsync(
            api.Client,
            "Original title",
            TaskState.Pending,
            "Denismay"
        );

        var updatePayload = new
        {
            title = "Updated title",
            description = "Updated description",
            status = TaskState.Done,
            responsible = "Denismay",
            dueDate = DueDate
        };

        var updateResponse = await api.Client.PutAsJsonAsync(
            $"/api/tasks/{created.Id}",
            updatePayload,
            JsonOptions
        );

        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode
        );

        var getResponse = await api.Client.GetAsync(
            $"/api/tasks/{created.Id}"
        );

        var updated = await getResponse.Content.ReadFromJsonAsync<TaskResponse>(
            JsonOptions
        );

        Assert.NotNull(updated);
        Assert.Equal("Updated title", updated.Title);
        Assert.Equal(TaskState.Done, updated.Status);
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_DeletesSuccessfully()
    {
        using var api = await TestApi.CreateAsync();

        var created = await CreateTaskAsync(
            api.Client,
            "Task to delete",
            TaskState.Pending,
            "Denismay"
        );

        var deleteResponse = await api.Client.DeleteAsync(
            $"/api/tasks/{created.Id}"
        );

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        var getResponse = await api.Client.GetAsync(
            $"/api/tasks/{created.Id}"
        );

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }

    [Fact]
    public async Task CreateTask_InvalidData_ReturnsBadRequest()
    {
        using var api = await TestApi.CreateAsync();

        var invalidPayload = new
        {
            title = "",
            description = "",
            responsible = "",
            dueDate = (DateTimeOffset?)null
        };

        var response = await api.Client.PostAsJsonAsync(
            "/api/tasks",
            invalidPayload,
            JsonOptions
        );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetTaskById_NonExistingTask_ReturnsNotFound()
    {
        using var api = await TestApi.CreateAsync();

        var response = await api.Client.GetAsync(
            $"/api/tasks/{Guid.NewGuid()}"
        );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetTasks_WithFiltersAndPagination_ReturnsCorrectResults()
    {
        using var api = await TestApi.CreateAsync();

        // Three matching tasks.
        await CreateTaskAsync(api.Client, "Task 1", TaskState.Pending, "Denismay");
        await CreateTaskAsync(api.Client, "Task 2", TaskState.Pending, "Denismay");
        await CreateTaskAsync(api.Client, "Task 3", TaskState.Pending, "Denismay");

        // These must be excluded by the filters.
        await CreateTaskAsync(api.Client, "Task 4", TaskState.Done, "Denismay");
        await CreateTaskAsync(api.Client, "Task 5", TaskState.Pending, "Another");

        var firstPageResponse = await api.Client.GetAsync(
            "/api/tasks?status=Pending&responsible=Denismay&page=1&pageSize=2"
        );

        Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);

        var firstPage =
            await firstPageResponse.Content.ReadFromJsonAsync<PaginatedTasksResponse>(
                JsonOptions
            );

        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(1, firstPage.Page);
        Assert.All(firstPage.Items, task =>
        {
            Assert.Equal(TaskState.Pending, task.Status);
            Assert.Equal("Denismay", task.Responsible);
        });

        var secondPageResponse = await api.Client.GetAsync(
            "/api/tasks?status=Pending&responsible=Denismay&page=2&pageSize=2"
        );

        var secondPage =
            await secondPageResponse.Content.ReadFromJsonAsync<PaginatedTasksResponse>(
                JsonOptions
            );

        Assert.NotNull(secondPage);
        Assert.Single(secondPage.Items);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Equal(2, secondPage.Page);
    }

    [Theory]
    [InlineData("/api/tasks?page=0&pageSize=10")]
    [InlineData("/api/tasks?page=1&pageSize=101")]
    public async Task GetTasks_InvalidPagination_ReturnsBadRequest(string url)
    {
        using var api = await TestApi.CreateAsync();

        var response = await api.Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<TaskResponse> CreateTaskAsync(
        HttpClient client,
        string title,
        TaskState status,
        string responsible)
    {
        var payload = new
        {
            title,
            description = $"Description for {title}",
            status,
            responsible,
            dueDate = DueDate
        };

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            payload,
            JsonOptions
        );

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TaskResponse>(
            JsonOptions
        );

        Assert.NotNull(created);

        return created;
    }
}