using System.Text;
using file_service.Data;
using file_service.DTOs;
using file_service.Models;
using file_service.Repositories.Interfaces;
using file_service.Services;
using file_service.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FileModel = file_service.Models.File;

namespace file_service.Tests.Services;

public class FileServiceTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly FileDbContext _dbContext;
    private readonly FileService _fileService;
    private readonly string _tempPath;
    private readonly string _finalPath;

    public FileServiceTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockKafkaProducer = new Mock<IKafkaProducerService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<FileService>>();

        // Setup In-Memory Database with Transaction Ignored
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new FileDbContext(options);

        // Setup temporary storage paths
        var basePath = Path.Combine(Path.GetTempPath(), "spm_tests", Guid.NewGuid().ToString());
        _tempPath = Path.Combine(basePath, "temp");
        _finalPath = Path.Combine(basePath, "final");

        _mockConfiguration.Setup(c => c["FileStorage:Path"]).Returns(basePath);

        _fileService = new FileService(
            _dbContext,
            _mockFileRepository.Object,
            _mockKafkaProducer.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ReturnsFileResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var content = "Hello World";
        var fileName = "test.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        _mockFileRepository.Setup(r => r.CreateAsync(It.IsAny<FileModel>()))
            .Callback<FileModel>(f => 
            {
                f.Id = Guid.NewGuid(); // Simulate DB generating ID
                _dbContext.Files.Add(f); // Add to in-memory DB so SaveChanges works
            })
            .Returns((FileModel f) => Task.FromResult(f));

        // Act
        var result = await _fileService.UploadFileAsync(userId, formFile);

        // Assert
        result.Should().NotBeNull();
        result.OriginalName.Should().Be(fileName);
        result.Size.Should().Be(stream.Length);
        
        // Verify file was created on disk
        // We need to capture the file model to get the storage path
        var capturedFile = _dbContext.Files.Local.FirstOrDefault();
        capturedFile.Should().NotBeNull();
        System.IO.File.Exists(capturedFile!.StoragePath).Should().BeTrue();
        
        // Verify repository called
        _mockFileRepository.Verify(r => r.CreateAsync(It.IsAny<FileModel>()), Times.Once);
        
        // Verify Kafka event published
        _mockKafkaProducer.Verify(k => k.PublishFileUploadedAsync(It.IsAny<Guid>(), userId, fileName, stream.Length), Times.Once);

        // Cleanup
        if (Directory.Exists(Path.GetDirectoryName(_tempPath)))
            Directory.Delete(Path.GetDirectoryName(_tempPath)!, true);
    }

    [Fact]
    public async Task UploadFileAsync_EmptyFile_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream();
        var formFile = new FormFile(stream, 0, 0, "file", "empty.txt");

        // Act
        var action = async () => await _fileService.UploadFileAsync(userId, formFile);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File is empty or null*");
    }

    [Fact]
    public async Task GetFileByIdAsync_FileExists_ReturnsFileResponse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var file = new FileModel
        {
            Id = fileId,
            OriginalName = "test.txt",
            StoredName = "stored_test.txt",
            StoragePath = "/tmp/test.txt",
            Size = 100,
            MimeType = "text/plain",
            UploadedBy = Guid.NewGuid(),
            UploadedAt = DateTime.UtcNow
        };

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync(file);

        // Act
        var result = await _fileService.GetFileByIdAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(fileId);
        result.OriginalName.Should().Be("test.txt");
    }

    [Fact]
    public async Task GetFileByIdAsync_FileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync((FileModel?)null);

        // Act
        var result = await _fileService.GetFileByIdAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_FileExistsAndUserIsOwner_ReturnsTrue()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var filePath = Path.Combine(_finalPath, "delete_me.txt");
        
        // Create dummy file
        Directory.CreateDirectory(_finalPath);
        await System.IO.File.WriteAllTextAsync(filePath, "content");

        var file = new FileModel
        {
            Id = fileId,
            UploadedBy = userId,
            StoragePath = filePath,
            IsDeleted = false
        };

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync(file);
        _mockFileRepository.Setup(r => r.DeleteAsync(fileId)).ReturnsAsync(true);

        // Act
        var result = await _fileService.DeleteFileAsync(fileId, userId);

        // Assert
        result.Should().BeTrue();
        _mockFileRepository.Verify(r => r.DeleteAsync(fileId), Times.Once);
        System.IO.File.Exists(filePath).Should().BeFalse();

        // Cleanup
        if (Directory.Exists(Path.GetDirectoryName(_tempPath)))
            Directory.Delete(Path.GetDirectoryName(_tempPath)!, true);
    }

    [Fact]
    public async Task DeleteFileAsync_UserIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var file = new FileModel
        {
            Id = fileId,
            UploadedBy = otherUserId // Different user
        };

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId)).ReturnsAsync(file);

        // Act
        var action = async () => await _fileService.DeleteFileAsync(fileId, userId);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
        _mockFileRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
