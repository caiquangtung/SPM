using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_service.DTOs.Projects;
using project_service.Extensions;
using project_service.Services.Interfaces;

namespace project_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProjects(CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var result = await _projects.GetMyProjectsAsync(userId, cancellationToken);
        return this.OkResponse(result, "Projects retrieved successfully");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projects.GetByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return this.NotFoundResponse("Project not found", "PROJECT_NOT_FOUND");
        }
        return this.OkResponse(result, "Project retrieved successfully");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var result = await _projects.CreateAsync(userId, request, cancellationToken);
        return this.CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Project created successfully");
    }
}


