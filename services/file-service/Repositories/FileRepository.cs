using Microsoft.EntityFrameworkCore;
using file_service.Data;
using FileModel = file_service.Models.File;
using file_service.Repositories.Interfaces;

namespace file_service.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FileDbContext _context;

    public FileRepository(FileDbContext context)
    {
        _context = context;
    }

    public async Task<FileModel?> GetByIdAsync(Guid id)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
    }

    public async Task<IEnumerable<FileModel>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Files
            .Where(f => f.UploadedBy == userId && !f.IsDeleted)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();
    }

    public Task<FileModel> CreateAsync(FileModel file)
    {
        _context.Files.Add(file);
        return Task.FromResult(file);
    }

    public Task<FileModel> UpdateAsync(FileModel file)
    {
        file.UploadedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        return Task.FromResult(file);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var file = await GetByIdAsync(id);
        if (file == null)
            return false;

        file.IsDeleted = true;
        _context.Files.Update(file);
        return true;
    }
}

