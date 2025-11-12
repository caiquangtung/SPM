namespace project_service.DTOs.Tasks;

public class SearchTasksRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 10;
    public Guid? ProjectId { get; set; }
}

