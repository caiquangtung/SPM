using file_service.DTOs;
using Microsoft.AspNetCore.Http;

namespace file_service.Services.Interfaces;

public interface IFileService
{
    Task<FileResponse> UploadFileAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    Task<FileResponse?> GetFileByIdAsync(Guid id);
    Task<byte[]?> DownloadFileAsync(Guid id);
    Task<FileStream?> GetFileStreamAsync(Guid id);
    Task<bool> DeleteFileAsync(Guid id, Guid userId);
    Task<IEnumerable<FileResponse>> GetUserFilesAsync(Guid userId);
}

