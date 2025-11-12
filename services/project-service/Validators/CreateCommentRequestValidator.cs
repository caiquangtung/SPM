using FluentValidation;
using project_service.DTOs.Comments;

namespace project_service.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .RequiredNonEmpty("Comment content")
            .MaximumLength(5000).WithMessage("Comment content must not exceed 5000 characters");
    }
}

