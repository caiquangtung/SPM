using Microsoft.EntityFrameworkCore;

namespace file_service.Data;

public static class SeedData
{
    public static void EnsureSeedData(FileDbContext context, ILogger logger, IConfiguration configuration)
    {
        // Ensure storage directory exists
        var storagePath = configuration["FileStorage:Path"] ?? "/app/storage";
        
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
            logger.LogInformation("Created file storage directory at {StoragePath}", storagePath);
        }

        // File service typically doesn't need seed data
        // Files and attachments are created through API calls
        logger.LogInformation("File service storage ready. No seed data required.");
    }
}
