using file_service.Data;
using file_service.DTOs;
using file_service.Models;
using file_service.Repositories.Interfaces;
using file_service.Services.Interfaces;

namespace file_service.Services;

public class TaskAttachmentService : ITaskAttachmentService
{
    private readonly FileDbContext _db;
    private readonly ITaskAttachmentRepository _taskAttachmentRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<TaskAttachmentService> _logger;

    public TaskAttachmentService(
        FileDbContext db,
        ITaskAttachmentRepository taskAttachmentRepository,
        IFileRepository fileRepository,
        ILogger<TaskAttachmentService> logger)
    {
        _db = db;
        _taskAttachmentRepository = taskAttachmentRepository;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task<TaskAttachmentResponse> AttachFileToTaskAsync(Guid userId, Guid taskId, Guid fileId, CancellationToken cancellationToken = default)
    {
        // Verify file exists and belongs to user
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            throw new KeyNotFoundException("File not found");
        }

        if (file.UploadedBy != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to attach this file");
        }

        // Check if attachment already exists
        var existingAttachments = await _taskAttachmentRepository.GetByTaskIdAsync(taskId);
        if (existingAttachments.Any(ta => ta.FileId == fileId))
        {
            throw new InvalidOperationException("File is already attached to this task");
        }

        var attachment = new TaskAttachment
        {
            TaskId = taskId,
            FileId = fileId,
            UploadedBy = userId
        };

        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _ = _taskAttachmentRepository.CreateAsync(attachment);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Reload with File navigation property
            var savedAttachment = await _taskAttachmentRepository.GetByIdAsync(attachment.Id);
            if (savedAttachment == null)
            {
                throw new InvalidOperationException("Failed to retrieve saved attachment");
            }

            _logger.LogInformation("File {FileId} attached to task {TaskId} by user {UserId}", fileId, taskId, userId);
            return TaskAttachmentResponse.FromEntity(savedAttachment);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<TaskAttachmentResponse>> GetTaskAttachmentsAsync(Guid taskId)
    {
        var attachments = await _taskAttachmentRepository.GetByTaskIdAsync(taskId);
        return attachments.Select(TaskAttachmentResponse.FromEntity);
    }

    public async Task<bool> DetachFileFromTaskAsync(Guid attachmentId, Guid userId)
    {
        var attachment = await _taskAttachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            return false;
        }

        // Check ownership
        if (attachment.UploadedBy != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to detach this file");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _taskAttachmentRepository.DeleteAsync(attachmentId);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("File detached from task: {AttachmentId} by user {UserId}", attachmentId, userId);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

