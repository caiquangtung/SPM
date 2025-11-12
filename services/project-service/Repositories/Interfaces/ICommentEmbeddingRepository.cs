using project_service.Models;

namespace project_service.Repositories.Interfaces;

public interface ICommentEmbeddingRepository
{
    Task<CommentEmbedding?> GetByCommentIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<CommentEmbedding> CreateOrUpdateAsync(CommentEmbedding embedding);
    Task<bool> DeleteAsync(Guid commentId, CancellationToken cancellationToken = default);
}

