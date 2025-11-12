using Microsoft.AspNetCore.Mvc;
using project_service.DTOs.Tasks;
using project_service.Services.Interfaces;

namespace project_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;

    public TasksController(ITaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProject([FromQuery] Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _tasks.GetByProjectAsync(projectId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _tasks.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByProject), new { projectId = result.ProjectId }, result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _tasks.UpdateStatusAsync(id, request, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    private Guid GetUserId()
    {
        // Placeholder until JWT wired: use a deterministic fake user for dev
        return Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }
}


