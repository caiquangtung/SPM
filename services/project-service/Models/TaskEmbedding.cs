namespace project_service.Models;

public class TaskEmbedding
{
    public Guid TaskId { get; set; }
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectTask? Task { get; set; }
}

