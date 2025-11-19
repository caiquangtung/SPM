using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using file_service.Extensions;
using file_service.Services.Interfaces;

namespace file_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return this.BadRequestResponse("File is required", "FILE_REQUIRED");
        }

        try
        {
            var userId = this.GetUserId();
            var result = await _fileService.UploadFileAsync(userId, file, cancellationToken);
            return this.OkResponse(result, "File uploaded successfully");
        }
        catch (ArgumentException ex)
        {
            return this.BadRequestResponse(ex.Message, "INVALID_FILE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            throw;
        }
    }

    /// <summary>
    /// Get file metadata by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var file = await _fileService.GetFileByIdAsync(id);
        if (file == null)
        {
            return this.NotFoundResponse("File not found", "FILE_NOT_FOUND");
        }

        return this.OkResponse(file, "File retrieved successfully");
    }

    /// <summary>
    /// Download a file by ID (optimized for large files - uses streaming)
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var file = await _fileService.GetFileByIdAsync(id);
        if (file == null)
        {
            return this.NotFoundResponse("File not found", "FILE_NOT_FOUND");
        }

        // For files larger than 10MB, use streaming for better memory efficiency
        if (file.Size > 10 * 1024 * 1024)
        {
            var fileStream = await _fileService.GetFileStreamAsync(id);
            if (fileStream == null)
            {
                return this.NotFoundResponse("File not found on disk", "FILE_NOT_FOUND");
            }

            return File(fileStream, file.MimeType, file.OriginalName, enableRangeProcessing: true);
        }

        // For smaller files, use in-memory download (backward compatible)
        var fileBytes = await _fileService.DownloadFileAsync(id);
        if (fileBytes == null)
        {
            return this.NotFoundResponse("File not found on disk", "FILE_NOT_FOUND");
        }

        return File(fileBytes, file.MimeType, file.OriginalName);
    }

    /// <summary>
    /// Delete a file by ID
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        try
        {
            var userId = this.GetUserId();
            var deleted = await _fileService.DeleteFileAsync(id, userId);
            if (!deleted)
            {
                return this.NotFoundResponse("File not found", "FILE_NOT_FOUND");
            }

            return this.OkResponse("File deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.UnauthorizedResponse(ex.Message, "UNAUTHORIZED");
        }
    }

    /// <summary>
    /// Get all files uploaded by the current user
    /// </summary>
    [HttpGet("my-files")]
    public async Task<IActionResult> GetMyFiles()
    {
        var userId = this.GetUserId();
        var files = await _fileService.GetUserFilesAsync(userId);
        return this.OkResponse(files, "Files retrieved successfully");
    }
}

