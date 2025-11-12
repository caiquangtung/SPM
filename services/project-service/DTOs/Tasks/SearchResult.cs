namespace project_service.DTOs.Tasks;

public class SearchResult
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Similarity { get; set; }
}

