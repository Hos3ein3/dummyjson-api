using Application.Common.Errors;
using FluentValidation;
using SharedKernel.Results;

namespace Application.Common.Validation;

/// <summary>
/// Extension methods for mapping FluentValidation results into Result objects.
/// </summary>
public static class FluentValidationExtensions
{
    public static async Task<Result> ValidateToResultAsync<T>(
        this IValidator<T> validator,
        T model,
        IErrorFactory errorFactory,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(model, ct);

        if (validationResult.IsValid)
        {
            return Result.Success();
        }

        var errors = validationResult.Errors
            .Select(f => Error.Validation(f.ErrorCode ?? "Validation.Error", f.ErrorMessage))
            .ToList();

        return Result.Failure(Error.ValidationSummary(
            "Validation.Summary",
            "One or more validation errors occurred.",
            errors));
    }
}
