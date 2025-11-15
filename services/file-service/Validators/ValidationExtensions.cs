using FluentValidation;

namespace file_service.Validators;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }

    public static IRuleBuilderOptions<T, string> RequiredPassword<T>(this IRuleBuilder<T, string> ruleBuilder, int minLength = 6)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(minLength).WithMessage($"Password must be at least {minLength} characters long");
    }
}

