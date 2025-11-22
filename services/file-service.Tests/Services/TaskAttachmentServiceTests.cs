using file_service.Data;
using file_service.DTOs;
using file_service.Models;
using file_service.Repositories.Interfaces;
using file_service.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FileModel = file_service.Models.File;

namespace file_service.Tests.Services;

public class TaskAttachmentServiceTests
{
    private readonly Mock<ITaskAttachmentRepository> _mockTaskAttachmentRepository;
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<ILogger<TaskAttachmentService>> _mockLogger;
    private readonly FileDbContext _dbContext;
    private readonly TaskAttachmentService _service;

    public TaskAttachmentServiceTests()
    {
        _mockTaskAttachmentRepository = new Mock<ITaskAttachmentRepository>();
        _mockFileRepository = new Mock<IFileRepository>();
        _mockLogger = new Mock<ILogger<TaskAttachmentService>>();

        // Setup In-Memory Database with Transaction Ignored
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new FileDbContext(options);

        _service = new TaskAttachmentService(
            _dbContext,
            _mockTaskAttachmentRepository.Object,
            _mockFileRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task AttachFileToTaskAsync_ValidRequest_ReturnsAttachmentResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var file = new FileModel
        {
            Id = fileId,
            UploadedBy = userId,
            OriginalName = "test.pdf"
        };

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync(file);
        _mockTaskAttachmentRepository.Setup(r => r.GetByTaskIdAsync(taskId)).ReturnsAsync(new List<TaskAttachment>());
        
        _mockTaskAttachmentRepository.Setup(r => r.CreateAsync(It.IsAny<TaskAttachment>()))
            .Callback<TaskAttachment>(ta => 
            {
                ta.Id = Guid.NewGuid();
                // We need to mock GetByIdAsync to return this attachment after save
                _mockTaskAttachmentRepository.Setup(r => r.GetByIdAsync(ta.Id))
                    .ReturnsAsync(new TaskAttachment 
                    { 
                        Id = ta.Id, 
                        TaskId = taskId, 
                        FileId = fileId, 
                        UploadedBy = userId,
                        File = file // Include navigation property
                    });
            })
            .Returns((TaskAttachment ta) => Task.FromResult(ta));

        // Act
        var result = await _service.AttachFileToTaskAsync(userId, taskId, fileId);

        // Assert
        result.Should().NotBeNull();
        result.TaskId.Should().Be(taskId);
        result.FileId.Should().Be(fileId);
        
        _mockTaskAttachmentRepository.Verify(r => r.CreateAsync(It.IsAny<TaskAttachment>()), Times.Once);
    }

    [Fact]
    public async Task AttachFileToTaskAsync_FileNotBelongToUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var file = new FileModel
        {
            Id = fileId,
            UploadedBy = otherUserId // Different user
        };

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync(file);

        // Act
        var action = async () => await _service.AttachFileToTaskAsync(userId, taskId, fileId);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to attach this file");
    }

    [Fact]
    public async Task AttachFileToTaskAsync_FileAlreadyAttached_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var file = new FileModel { Id = fileId, UploadedBy = userId };
        var existingAttachment = new TaskAttachment { TaskId = taskId, FileId = fileId };

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync(file);
        _mockTaskAttachmentRepository.Setup(r => r.GetByTaskIdAsync(taskId))
            .ReturnsAsync(new List<TaskAttachment> { existingAttachment });

        // Act
        var action = async () => await _service.AttachFileToTaskAsync(userId, taskId, fileId);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("File is already attached to this task");
    }

    [Fact]
    public async Task DetachFileFromTaskAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        var attachment = new TaskAttachment
        {
            Id = attachmentId,
            UploadedBy = userId
        };

        _mockTaskAttachmentRepository.Setup(r => r.GetByIdAsync(attachmentId)).ReturnsAsync(attachment);
        _mockTaskAttachmentRepository.Setup(r => r.DeleteAsync(attachmentId)).ReturnsAsync(true);

        // Act
        var result = await _service.DetachFileFromTaskAsync(attachmentId, userId);

        // Assert
        result.Should().BeTrue();
        _mockTaskAttachmentRepository.Verify(r => r.DeleteAsync(attachmentId), Times.Once);
    }
}
