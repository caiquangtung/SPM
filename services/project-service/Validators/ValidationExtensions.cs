using FluentValidation;

namespace project_service.Validators;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredNonEmpty<T>(this IRuleBuilder<T, string> rule, string fieldDisplay)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldDisplay} is required")
            .MaximumLength(500).WithMessage($"{fieldDisplay} must not exceed 500 characters");
    }

    public static IRuleBuilderOptions<T, string?> MaxLengthIfNotNull<T>(this IRuleBuilder<T, string?> rule, int max, string fieldDisplay)
    {
        return rule.MaximumLength(max).WithMessage($"{fieldDisplay} must be at most {max} characters");
    }

    public static IRuleBuilderOptions<T, Guid> RequiredGuid<T>(this IRuleBuilder<T, Guid> rule, string fieldDisplay)
    {
        return rule.NotEmpty().WithMessage($"{fieldDisplay} is required");
    }
}

