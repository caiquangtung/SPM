using FluentValidation;
using project_service.DTOs.Tasks;

namespace project_service.Validators;

public class SearchTasksRequestValidator : AbstractValidator<SearchTasksRequest>
{
    public SearchTasksRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search query is required")
            .MaximumLength(1000).WithMessage("Search query must not exceed 1000 characters");

        RuleFor(x => x.TopK)
            .GreaterThan(0).WithMessage("TopK must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("TopK must not exceed 100");
    }
}

