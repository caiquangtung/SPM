using FluentValidation;
using user_service.DTOs;

namespace user_service.Validators;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .RequiredNonEmpty("Verification token");
    }
}

