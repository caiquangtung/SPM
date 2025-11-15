using FluentValidation;
using file_service.DTOs;

namespace file_service.Validators;

public class AttachFileToTaskRequestValidator : AbstractValidator<AttachFileToTaskRequest>
{
    public AttachFileToTaskRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required");

        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");
    }
}

