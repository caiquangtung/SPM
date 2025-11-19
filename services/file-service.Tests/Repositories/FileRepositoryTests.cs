using System.Linq;
using file_service.Data;
using file_service.Repositories;
using FileModel = file_service.Models.File;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace file_service.Tests.Repositories;

public class FileRepositoryTests
{
    private static FileDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FileDbContext(options);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyActiveFilesOrderedByUploadDate()
    {
        await using var context = CreateDbContext();
        var repository = new FileRepository(context);
        var userId = Guid.NewGuid();

        context.Files.AddRange(
            new FileModel
            {
                UploadedBy = userId,
                OriginalName = "old.pdf",
                StoredName = "old",
                MimeType = "application/pdf",
                Size = 10,
                StoragePath = "/tmp/old",
                UploadedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new FileModel
            {
                UploadedBy = userId,
                OriginalName = "new.pdf",
                StoredName = "new",
                MimeType = "application/pdf",
                Size = 20,
                StoragePath = "/tmp/new",
                UploadedAt = DateTime.UtcNow
            },
            new FileModel
            {
                UploadedBy = userId,
                OriginalName = "deleted.pdf",
                StoredName = "deleted",
                MimeType = "application/pdf",
                Size = 5,
                StoragePath = "/tmp/deleted",
                IsDeleted = true
            },
            new FileModel
            {
                UploadedBy = Guid.NewGuid(),
                OriginalName = "other-user.pdf",
                StoredName = "other",
                MimeType = "application/pdf",
                Size = 5,
                StoragePath = "/tmp/other"
            }
        );
        await context.SaveChangesAsync();

        var results = (await repository.GetByUserIdAsync(userId)).ToList();

        results.Should().HaveCount(2);
        results[0].OriginalName.Should().Be("new.pdf");
        results[1].OriginalName.Should().Be("old.pdf");
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesAndReturnsTrue()
    {
        await using var context = CreateDbContext();
        var repository = new FileRepository(context);
        var fileId = Guid.NewGuid();

        context.Files.Add(new FileModel
        {
            Id = fileId,
            UploadedBy = Guid.NewGuid(),
            OriginalName = "doc.txt",
            StoredName = "doc",
            MimeType = "text/plain",
            Size = 1,
            StoragePath = "/tmp/doc"
        });
        await context.SaveChangesAsync();

        var deleted = await repository.DeleteAsync(fileId);

        deleted.Should().BeTrue();
        var entity = await context.Files.FirstAsync(f => f.Id == fileId);
        entity.IsDeleted.Should().BeTrue();
    }
}
