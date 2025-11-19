using file_service.Data;
using file_service.DTOs;
using FileModel = file_service.Models.File;
using file_service.Repositories.Interfaces;
using file_service.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace file_service.Services;

public class FileService : IFileService
{
    private readonly FileDbContext _db;
    private readonly IFileRepository _fileRepository;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly ILogger<FileService> _logger;
    private readonly string _storagePath;
    private readonly string _tempPath;
    private readonly string _finalPath;

    public FileService(
        FileDbContext db,
        IFileRepository fileRepository,
        IKafkaProducerService kafkaProducer,
        IConfiguration configuration,
        ILogger<FileService> logger)
    {
        _db = db;
        _fileRepository = fileRepository;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _storagePath = configuration["FileStorage:Path"] ?? "/app/storage";
        _tempPath = Path.Combine(_storagePath, "temp");
        _finalPath = Path.Combine(_storagePath, "final");

        // Ensure storage directories exist
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }
        if (!Directory.Exists(_finalPath))
        {
            Directory.CreateDirectory(_finalPath);
        }
    }

    public async Task<FileResponse> UploadFileAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        // Phase 1: Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        var maxFileSize = 100 * 1024 * 1024; // 100 MB
        if (file.Length > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)} MB", nameof(file));
        }

        // Phase 2: Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{fileExtension}";
        var tempPath = Path.Combine(_tempPath, storedName);
        var finalPath = Path.Combine(_finalPath, storedName);

        // Phase 3: Save file to temporary location (streaming to save memory)
        // Adaptive buffer size: larger buffer for larger files to reduce I/O operations
        // Buffer size: min 8KB, max 1MB, scales with file size
        var bufferSize = Math.Min(Math.Max(8192, (int)(file.Length / 100)), 1048576);
        try
        {
            using (var stream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.SequentialScan | FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Phase 4: Begin Database Transaction
            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Phase 5: Save metadata to database (with final path)
                var fileEntity = new FileModel
                {
                    OriginalName = file.FileName,
                    StoredName = storedName,
                    MimeType = file.ContentType,
                    Size = file.Length,
                    StoragePath = finalPath, // Store final path in database
                    UploadedBy = userId
                };

                _ = _fileRepository.CreateAsync(fileEntity);
                await _db.SaveChangesAsync(cancellationToken);

                // Phase 6: Commit Database Transaction
                await transaction.CommitAsync(cancellationToken);

                // Phase 7: Move file from temp to final location (atomic operation, very fast)
                // This is a fast operation as it's just a pointer change on the same filesystem
                try
                {
                    System.IO.File.Move(tempPath, finalPath, overwrite: false);
                }
                catch (Exception moveEx)
                {
                    // Critical: DB committed but file move failed
                    // Fallback: Try copy + delete as backup strategy
                    _logger.LogError(moveEx, "Failed to move file from temp to final location. Attempting copy fallback. FileId: {FileId}, TempPath: {TempPath}, FinalPath: {FinalPath}",
                        fileEntity.Id, tempPath, finalPath);

                    try
                    {
                        System.IO.File.Copy(tempPath, finalPath, overwrite: false);
                        System.IO.File.Delete(tempPath);
                        _logger.LogInformation("Successfully used copy fallback for file: {FileId}", fileEntity.Id);
                    }
                    catch (Exception copyEx)
                    {
                        // Critical error: DB committed but file cannot be moved/copied
                        _logger.LogCritical(copyEx,
                            "CRITICAL: Database transaction committed but file cannot be moved to final location. " +
                            "FileId: {FileId}, TempPath: {TempPath}, FinalPath: {FinalPath}. " +
                            "Manual intervention required!",
                            fileEntity.Id, tempPath, finalPath);
                        throw new InvalidOperationException(
                            "File upload committed to database but failed to move to final storage location. " +
                            "Please contact administrator.", copyEx);
                    }
                }

                // Phase 8: Publish Kafka event after transaction commit (fire-and-forget)
                _ = _kafkaProducer.PublishFileUploadedAsync(fileEntity.Id, userId, fileEntity.OriginalName, fileEntity.Size);

                _logger.LogInformation("File uploaded successfully: {FileId} by user {UserId}", fileEntity.Id, userId);
                return FileResponse.FromEntity(fileEntity);
            }
            catch
            {
                // Phase 9: Rollback transaction if database operations fail
                await transaction.RollbackAsync(cancellationToken);

                // Clean up temp file if database save fails
                if (System.IO.File.Exists(tempPath))
                {
                    try
                    {
                        System.IO.File.Delete(tempPath);
                        _logger.LogInformation("Cleaned up temp file after transaction rollback: {TempPath}", tempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file after rollback: {TempPath}", tempPath);
                    }
                }
                throw;
            }
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            // Clean up temp file if file write fails
            if (System.IO.File.Exists(tempPath))
            {
                try
                {
                    System.IO.File.Delete(tempPath);
                    _logger.LogInformation("Cleaned up temp file after write failure: {TempPath}", tempPath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to delete temp file after write failure: {TempPath}", tempPath);
                }
            }
            throw;
        }
    }

    public async Task<FileResponse?> GetFileByIdAsync(Guid id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        return file != null ? FileResponse.FromEntity(file) : null;
    }

    public async Task<byte[]?> DownloadFileAsync(Guid id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null || !System.IO.File.Exists(file.StoragePath))
        {
            return null;
        }

        // For small files (< 10MB), read into memory is acceptable
        // For larger files, consider using streaming download (see GetFileStreamAsync)
        if (file.Size < 10 * 1024 * 1024)
        {
            return await System.IO.File.ReadAllBytesAsync(file.StoragePath);
        }

        // For larger files, use streaming to avoid memory issues
        // This method still returns byte[] for backward compatibility
        // Consider using GetFileStreamAsync for better performance with large files
        using var fileStream = new FileStream(
            file.StoragePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            8192,
            FileOptions.SequentialScan | FileOptions.Asynchronous);

        var buffer = new byte[file.Size];
        var bytesRead = 0;
        var totalBytesRead = 0;

        while (totalBytesRead < file.Size)
        {
            bytesRead = await fileStream.ReadAsync(buffer, totalBytesRead, (int)file.Size - totalBytesRead);
            if (bytesRead == 0) break;
            totalBytesRead += bytesRead;
        }

        return buffer;
    }

    /// <summary>
    /// Get file stream for efficient large file downloads (optimized version)
    /// </summary>
    public async Task<FileStream?> GetFileStreamAsync(Guid id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null || !System.IO.File.Exists(file.StoragePath))
        {
            return null;
        }

        // Return FileStream for streaming download (more memory efficient)
        return new FileStream(
            file.StoragePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            8192,
            FileOptions.SequentialScan | FileOptions.Asynchronous);
    }

    public async Task<bool> DeleteFileAsync(Guid id, Guid userId)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null)
        {
            return false;
        }

        // Check ownership
        if (file.UploadedBy != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this file");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _fileRepository.DeleteAsync(id);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Delete physical file
            if (System.IO.File.Exists(file.StoragePath))
            {
                System.IO.File.Delete(file.StoragePath);
            }

            _logger.LogInformation("File deleted: {FileId} by user {UserId}", id, userId);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<FileResponse>> GetUserFilesAsync(Guid userId)
    {
        var files = await _fileRepository.GetByUserIdAsync(userId);
        return files.Select(FileResponse.FromEntity);
    }
}

