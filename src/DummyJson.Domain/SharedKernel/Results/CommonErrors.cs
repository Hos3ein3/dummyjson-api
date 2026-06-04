namespace SharedKernel.Results;

/// <summary>
/// Contains common, hardcoded error definitions that do not require localization.
/// </summary>
public static class CommonErrors
{
    public static Error Required(string field) =>
        Error.Validation("General.Required", $"The field '{field}' is required.");

    public static Error Invalid(string field) =>
        Error.Validation("General.Invalid", $"The field '{field}' is invalid.");

    public static Error NotFound(string entity, object? key = null) =>
        Error.NotFound($"{entity}.NotFound", key is null ? $"The {entity} was not found." : $"The {entity} with key '{key}' was not found.");

    public static Error Duplicate(string entity, string field, object? value = null) =>
        Error.Conflict($"{entity}.Duplicate", value is null ? $"The {entity} has a duplicate '{field}'." : $"The {entity} already exists with '{field}': '{value}'.");

    public static Error Unauthorized() =>
        Error.Unauthorized("General.Unauthorized", "You are not authorized to perform this action.");

    public static Error Forbidden() =>
        Error.Forbidden("General.Forbidden", "You are forbidden from accessing this resource.");

    public static Error Unexpected(string? description = null) =>
        Error.Failure("General.Unexpected", description ?? "An unexpected error occurred.");
}
