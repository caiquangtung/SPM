using project_service.Models;

namespace project_service.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<ProjectComment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectComment>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<ProjectComment> CreateAsync(ProjectComment comment);
    Task<ProjectComment> UpdateAsync(ProjectComment comment);
}


