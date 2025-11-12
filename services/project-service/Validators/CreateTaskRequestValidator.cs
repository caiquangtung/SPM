using FluentValidation;
using project_service.DTOs.Tasks;

namespace project_service.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .RequiredGuid("Project ID");

        RuleFor(x => x.Title)
            .RequiredNonEmpty("Task title")
            .MaximumLength(500).WithMessage("Task title must not exceed 500 characters");

        RuleFor(x => x.Description)
            .MaxLengthIfNotNull(5000, "Description")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }
}

