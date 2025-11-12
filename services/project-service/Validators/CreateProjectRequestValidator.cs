using FluentValidation;
using project_service.DTOs.Projects;

namespace project_service.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredNonEmpty("Project name")
            .MaximumLength(255).WithMessage("Project name must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaxLengthIfNotNull(2000, "Description")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

