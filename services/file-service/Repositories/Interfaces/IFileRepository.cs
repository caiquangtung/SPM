using FileModel = file_service.Models.File;

namespace file_service.Repositories.Interfaces;

public interface IFileRepository
{
    Task<FileModel?> GetByIdAsync(Guid id);
    Task<IEnumerable<FileModel>> GetByUserIdAsync(Guid userId);
    Task<FileModel> CreateAsync(FileModel file);
    Task<FileModel> UpdateAsync(FileModel file);
    Task<bool> DeleteAsync(Guid id);
}

