using Microsoft.EntityFrameworkCore;
using file_service.Data;
using file_service.Models;
using file_service.Repositories.Interfaces;

namespace file_service.Repositories;

public class TaskAttachmentRepository : ITaskAttachmentRepository
{
    private readonly FileDbContext _context;

    public TaskAttachmentRepository(FileDbContext context)
    {
        _context = context;
    }

    public async Task<TaskAttachment?> GetByIdAsync(Guid id)
    {
        return await _context.TaskAttachments
            .Include(ta => ta.File)
            .FirstOrDefaultAsync(ta => ta.Id == id);
    }

    public async Task<IEnumerable<TaskAttachment>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.TaskAttachments
            .Include(ta => ta.File)
            .Where(ta => ta.TaskId == taskId && !ta.File.IsDeleted)
            .OrderByDescending(ta => ta.UploadedAt)
            .ToListAsync();
    }

    public Task<TaskAttachment> CreateAsync(TaskAttachment attachment)
    {
        _context.TaskAttachments.Add(attachment);
        return Task.FromResult(attachment);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var attachment = await GetByIdAsync(id);
        if (attachment == null)
            return false;

        _context.TaskAttachments.Remove(attachment);
        return true;
    }
}

