using Pgvector;

namespace project_service.Models;

public class TaskEmbedding
{
    public Guid TaskId { get; set; }
    public Vector Embedding { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectTask? Task { get; set; }
}

