using project_service.DTOs.Projects;

namespace project_service.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectResponse>> GetMyProjectsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> CreateAsync(Guid userId, CreateProjectRequest request, CancellationToken cancellationToken = default);
}


