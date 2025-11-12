using project_service.DTOs.Comments;

namespace project_service.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentResponse>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<CommentResponse> CreateAsync(Guid taskId, Guid userId, CreateCommentRequest request, CancellationToken cancellationToken = default);
}


