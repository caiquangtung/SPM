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

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<FileResponse> UploadFileAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        var maxFileSize = 100 * 1024 * 1024; // 100 MB
        if (file.Length > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)} MB", nameof(file));
        }

        // Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{fileExtension}";
        var storagePath = Path.Combine(_storagePath, storedName);

        // Save file to disk
        using (var stream = new FileStream(storagePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Create file entity
        var fileEntity = new FileModel
        {
            OriginalName = file.FileName,
            StoredName = storedName,
            MimeType = file.ContentType,
            Size = file.Length,
            StoragePath = storagePath,
            UploadedBy = userId
        };

        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _ = _fileRepository.CreateAsync(fileEntity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Publish event after transaction commit (fire-and-forget)
            _ = _kafkaProducer.PublishFileUploadedAsync(fileEntity.Id, userId, fileEntity.OriginalName, fileEntity.Size);

            _logger.LogInformation("File uploaded successfully: {FileId} by user {UserId}", fileEntity.Id, userId);
            return FileResponse.FromEntity(fileEntity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            // Clean up file if database save fails
            if (System.IO.File.Exists(storagePath))
            {
                System.IO.File.Delete(storagePath);
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

        return await System.IO.File.ReadAllBytesAsync(file.StoragePath);
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

