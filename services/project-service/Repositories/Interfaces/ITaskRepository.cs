using project_service.Models;

namespace project_service.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectTask>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectTask> CreateAsync(ProjectTask task);
    Task<ProjectTask> UpdateAsync(ProjectTask task);
}


