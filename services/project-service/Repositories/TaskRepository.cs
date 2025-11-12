using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models;
using project_service.Repositories.Interfaces;

namespace project_service.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ProjectDbContext _context;

    public TaskRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }

    public async Task<IEnumerable<ProjectTask>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ProjectTask> CreateAsync(ProjectTask task)
    {
        _context.Tasks.Add(task);
        return Task.FromResult(task);
    }

    public Task<ProjectTask> UpdateAsync(ProjectTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        return Task.FromResult(task);
    }
}


