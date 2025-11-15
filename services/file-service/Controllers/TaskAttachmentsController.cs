using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using file_service.DTOs;
using file_service.Extensions;
using file_service.Services.Interfaces;
using file_service.Validators;

namespace file_service.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/attachments")]
[Authorize]
public class TaskAttachmentsController : ControllerBase
{
    private readonly ITaskAttachmentService _taskAttachmentService;
    private readonly ILogger<TaskAttachmentsController> _logger;

    public TaskAttachmentsController(
        ITaskAttachmentService taskAttachmentService,
        ILogger<TaskAttachmentsController> logger)
    {
        _taskAttachmentService = taskAttachmentService;
        _logger = logger;
    }

    /// <summary>
    /// Attach a file to a task
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AttachFileToTask(
        [FromRoute] Guid taskId,
        [FromBody] AttachFileToTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TaskId != taskId)
        {
            return this.BadRequestResponse("Task ID in route and body must match", "INVALID_REQUEST");
        }

        var validator = new AttachFileToTaskRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.BadRequestResponse(
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                "VALIDATION_ERROR"
            );
        }

        try
        {
            var userId = this.GetUserId();
            var result = await _taskAttachmentService.AttachFileToTaskAsync(userId, taskId, request.FileId, cancellationToken);
            return this.OkResponse(result, "File attached to task successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFoundResponse(ex.Message, "FILE_NOT_FOUND");
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.UnauthorizedResponse(ex.Message, "UNAUTHORIZED");
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestResponse(ex.Message, "INVALID_OPERATION");
        }
    }

    /// <summary>
    /// Get all attachments for a task
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTaskAttachments([FromRoute] Guid taskId)
    {
        var attachments = await _taskAttachmentService.GetTaskAttachmentsAsync(taskId);
        return this.OkResponse(attachments, "Attachments retrieved successfully");
    }

    /// <summary>
    /// Detach a file from a task
    /// </summary>
    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> DetachFileFromTask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid attachmentId)
    {
        try
        {
            var userId = this.GetUserId();
            var deleted = await _taskAttachmentService.DetachFileFromTaskAsync(attachmentId, userId);
            if (!deleted)
            {
                return this.NotFoundResponse("Attachment not found", "ATTACHMENT_NOT_FOUND");
            }

            return this.OkResponse("File detached from task successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.UnauthorizedResponse(ex.Message, "UNAUTHORIZED");
        }
    }
}

