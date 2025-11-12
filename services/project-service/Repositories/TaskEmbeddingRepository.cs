using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models;
using project_service.Repositories.Interfaces;

namespace project_service.Repositories;

public class TaskEmbeddingRepository : ITaskEmbeddingRepository
{
    private readonly ProjectDbContext _context;

    public TaskEmbeddingRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<TaskEmbedding?> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.TaskEmbeddings
            .FirstOrDefaultAsync(e => e.TaskId == taskId, cancellationToken);
    }

    public Task<TaskEmbedding> CreateOrUpdateAsync(TaskEmbedding embedding)
    {
        var existing = _context.TaskEmbeddings.Find(embedding.TaskId);
        if (existing != null)
        {
            existing.Embedding = embedding.Embedding;
            _context.TaskEmbeddings.Update(existing);
            return Task.FromResult(existing);
        }

        _context.TaskEmbeddings.Add(embedding);
        return Task.FromResult(embedding);
    }

    public async Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var embedding = await GetByTaskIdAsync(taskId, cancellationToken);
        if (embedding == null)
            return false;

        _context.TaskEmbeddings.Remove(embedding);
        return true;
    }
}

