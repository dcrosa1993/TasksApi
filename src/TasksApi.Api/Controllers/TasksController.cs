using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksApi.Api.Domain.Entities;
using TasksApi.Api.Domain.Enums;
using TasksApi.Api.DTOs.Tasks;
using TasksApi.Api.Infrastructure.Persistence;

namespace TasksApi.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController(AppDbContext dbContext) : ControllerBase
{
    // GET: api/tasks
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        var tasks = await dbContext
            .Tasks.AsNoTracking()
            .OrderBy(task => task.Id)
            .Select(task => new TaskResponse(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Responsible,
                task.DueDate
            ))
            .ToListAsync(cancellationToken);

        return Ok(tasks);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var task = await dbContext
            .Tasks.AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

        if (task is null)
        {
            return TaskNotFound(id);
        }

        return Ok(ToResponse(task));
    }

    // POST: api/tasks
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken
    )
    {
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = NormalizeDescription(request.Description),
            Status = request.Status ?? TaskState.Pending,
            Responsible = request.Responsible.Trim(),
            DueDate = request.DueDate!.Value,
        };

        dbContext.Tasks.Add(task);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(task);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken
    )
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(
            task => task.Id == id,
            cancellationToken
        );

        if (task is null)
        {
            return TaskNotFound(id);
        }

        task.Title = request.Title.Trim();
        task.Description = NormalizeDescription(request.Description);
        task.Status = request.Status!.Value;
        task.Responsible = request.Responsible.Trim();
        task.DueDate = request.DueDate!.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(
            task => task.Id == id,
            cancellationToken
        );

        if (task is null)
        {
            return TaskNotFound(id);
        }

        dbContext.Tasks.Remove(task);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private ObjectResult TaskNotFound(Guid id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Task not found",
            detail: $"No task was found with ID '{id}'."
        );
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    private static TaskResponse ToResponse(TaskItem task)
    {
        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Responsible,
            task.DueDate
        );
    }
}
