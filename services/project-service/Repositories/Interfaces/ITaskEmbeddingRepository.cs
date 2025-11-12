using project_service.Models;

namespace project_service.Repositories.Interfaces;

public interface ITaskEmbeddingRepository
{
    Task<TaskEmbedding?> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskEmbedding> CreateOrUpdateAsync(TaskEmbedding embedding);
    Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default);
}

