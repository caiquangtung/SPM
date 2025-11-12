using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models;
using project_service.Repositories.Interfaces;

namespace project_service.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ProjectDbContext _context;

    public CommentRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectComment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
    }

    public async Task<IEnumerable<ProjectComment>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ProjectComment> CreateAsync(ProjectComment comment)
    {
        _context.Comments.Add(comment);
        return Task.FromResult(comment);
    }

    public Task<ProjectComment> UpdateAsync(ProjectComment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        _context.Comments.Update(comment);
        return Task.FromResult(comment);
    }
}


