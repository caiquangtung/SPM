using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_service.DTOs.Comments;
using project_service.Extensions;
using project_service.Services.Interfaces;

namespace project_service.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/[controller]")]
[Authorize]
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
        return this.OkResponse(result, "Comments retrieved successfully");
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        var result = await _comments.CreateAsync(taskId, userId, request, cancellationToken);
        return this.OkResponse(result, "Comment created successfully");
    }
}


