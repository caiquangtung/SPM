using Microsoft.AspNetCore.Mvc;
using project_service.DTOs.Comments;
using project_service.Services.Interfaces;

namespace project_service.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _comments;

    public CommentsController(ICommentService comments)
    {
        _comments = comments;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTask(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _comments.GetByTaskAsync(taskId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _comments.CreateAsync(taskId, userId, request, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        // Placeholder until JWT wired: use a deterministic fake user for dev
        return Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }
}


