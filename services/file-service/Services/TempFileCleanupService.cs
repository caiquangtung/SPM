using file_service.Services.Interfaces;

namespace file_service.Services;

/// <summary>
/// Background service to cleanup orphaned temp files
/// Runs periodically to remove temp files older than a specified age
/// </summary>
public class TempFileCleanupService : BackgroundService
{
    private readonly ILogger<TempFileCleanupService> _logger;
    private readonly string _tempPath;
    private readonly TimeSpan _cleanupInterval;
    private readonly TimeSpan _tempFileMaxAge;

    public TempFileCleanupService(
        IConfiguration configuration,
        ILogger<TempFileCleanupService> logger)
    {
        _logger = logger;
        var storagePath = configuration["FileStorage:Path"] ?? "/app/storage";
        _tempPath = Path.Combine(storagePath, "temp");

        // Cleanup every hour
        _cleanupInterval = TimeSpan.FromHours(1);

        // Remove temp files older than 24 hours (should not happen in normal operation)
        _tempFileMaxAge = TimeSpan.FromHours(24);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TempFileCleanupService started. Cleanup interval: {Interval}, Max temp file age: {MaxAge}",
            _cleanupInterval, _tempFileMaxAge);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupTempFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temp file cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupTempFilesAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_tempPath))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var filesDeleted = 0;
        var totalSizeDeleted = 0L;

        try
        {
            var tempFiles = Directory.GetFiles(_tempPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in tempFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileAge = now - fileInfo.LastWriteTimeUtc;

                    if (fileAge > _tempFileMaxAge)
                    {
                        var fileSize = fileInfo.Length;
                        System.IO.File.Delete(filePath);
                        filesDeleted++;
                        totalSizeDeleted += fileSize;

                        _logger.LogInformation("Deleted orphaned temp file: {FilePath}, Age: {Age}, Size: {Size} bytes",
                            filePath, fileAge, fileSize);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {FilePath}", filePath);
                }
            }

            if (filesDeleted > 0)
            {
                _logger.LogInformation(
                    "Temp file cleanup completed. Files deleted: {Count}, Total size freed: {Size} bytes ({SizeMB} MB)",
                    filesDeleted, totalSizeDeleted, totalSizeDeleted / (1024.0 * 1024.0));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating temp files in directory: {TempPath}", _tempPath);
        }

        await Task.CompletedTask;
    }
}

