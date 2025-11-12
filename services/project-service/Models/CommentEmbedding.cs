namespace project_service.Models;

public class CommentEmbedding
{
    public Guid CommentId { get; set; }
    public Guid? TaskId { get; set; }
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectComment? Comment { get; set; }
    public ProjectTask? Task { get; set; }
}

