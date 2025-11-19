using project_service.Models;
using TaskStatusEnum = project_service.Models.TaskStatus;

namespace project_service.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectTask>> GetByProjectAsync(
        Guid projectId,
        TaskStatusEnum? status = null,
        Guid? assignedTo = null,
        CancellationToken cancellationToken = default);
    Task<ProjectTask> CreateAsync(ProjectTask task);
    Task<ProjectTask> UpdateAsync(ProjectTask task);
}
