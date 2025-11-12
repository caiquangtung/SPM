using project_service.Models;

namespace project_service.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
}

