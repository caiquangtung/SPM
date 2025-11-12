using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.DTOs.Projects;
using project_service.Models;
using project_service.Repositories.Interfaces;
using project_service.Services.Interfaces;

namespace project_service.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly ProjectDbContext _db;

    public ProjectService(IProjectRepository projects, ProjectDbContext db)
    {
        _projects = projects;
        _db = db;
    }

    public async Task<IEnumerable<ProjectResponse>> GetMyProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _projects.GetByUserIdAsync(userId, cancellationToken);
        return list.Select(ProjectResponse.FromEntity);
    }

    public async Task<ProjectResponse?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var entity = await _projects.GetByIdAsync(projectId, cancellationToken);
        return entity == null ? null : ProjectResponse.FromEntity(entity);
    }

    public async Task<ProjectResponse> CreateAsync(Guid userId, CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Project
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = userId
        };

        _projects.CreateAsync(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return ProjectResponse.FromEntity(entity);
    }
}


