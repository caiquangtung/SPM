using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models;
using project_service.Repositories.Interfaces;

namespace project_service.Repositories;

public class CommentEmbeddingRepository : ICommentEmbeddingRepository
{
    private readonly ProjectDbContext _context;

    public CommentEmbeddingRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<CommentEmbedding?> GetByCommentIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _context.CommentEmbeddings
            .FirstOrDefaultAsync(e => e.CommentId == commentId, cancellationToken);
    }

    public Task<CommentEmbedding> CreateOrUpdateAsync(CommentEmbedding embedding)
    {
        var existing = _context.CommentEmbeddings.Find(embedding.CommentId);
        if (existing != null)
        {
            existing.Embedding = embedding.Embedding;
            existing.TaskId = embedding.TaskId;
            _context.CommentEmbeddings.Update(existing);
            return Task.FromResult(existing);
        }

        _context.CommentEmbeddings.Add(embedding);
        return Task.FromResult(embedding);
    }

    public async Task<bool> DeleteAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var embedding = await GetByCommentIdAsync(commentId, cancellationToken);
        if (embedding == null)
            return false;

        _context.CommentEmbeddings.Remove(embedding);
        return true;
    }
}

