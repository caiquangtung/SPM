using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_service.DTOs.Tasks;
using project_service.Extensions;
using project_service.Services.Interfaces;

namespace project_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;

    public TasksController(ITaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProject([FromQuery] GetTasksQuery query, CancellationToken cancellationToken)
    {
        if (query.ProjectId == Guid.Empty)
        {
            return this.BadRequestResponse("projectId is required", "INVALID_PROJECT_ID");
        }

        var result = await _tasks.GetByProjectAsync(query.ProjectId, query.Status, query.AssignedTo, cancellationToken);
        return this.OkResponse(result, "Tasks retrieved successfully");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var result = await _tasks.CreateAsync(userId, request, cancellationToken);
        return this.CreatedResponse(nameof(GetByProject), new { projectId = result.ProjectId }, result, "Task created successfully");
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _tasks.UpdateStatusAsync(id, request, cancellationToken);
        if (result == null)
        {
            return this.NotFoundResponse("Task not found", "TASK_NOT_FOUND");
        }
        return this.OkResponse(result, "Task status updated successfully");
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchSimilar([FromBody] SearchTasksRequest request, CancellationToken cancellationToken)
    {
        var results = await _tasks.SearchSimilarAsync(request, cancellationToken);
        return this.OkResponse(results, "Search completed successfully");
    }
}
