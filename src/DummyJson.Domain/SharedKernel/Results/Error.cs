namespace SharedKernel.Results;

/// <summary>
/// Represents a generic error with a code, description, type, and optional nested errors.
/// </summary>
public record Error(string Code, string Description, ErrorType Type, IReadOnlyList<Error>? Errors = null)
{
    public static Func<string, string>? ResourceLookup { get; set; }

    private static string GetDescription(string code, string? description)
    {
        if (!string.IsNullOrWhiteSpace(description)) return description;
        if (ResourceLookup != null) return ResourceLookup(code) ?? code;
        return code;
    }
    /// <summary>
    /// Represents a successful state without any errors.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string? description = null) =>
        new(code, GetDescription(code, description), ErrorType.Failure);

    public static Error Validation(string code, string? description = null) =>
        new(code, GetDescription(code, description), ErrorType.Validation);

    public static Error NotFound(string code, string? description = null) =>
        new(code, GetDescription(code, description), ErrorType.NotFound);

    public static Error Conflict(string code, string? description = null) =>
        new(code, GetDescription(code, description), ErrorType.Conflict);

    public static Error Unauthorized(string code, string? description = null) =>
        new(code, GetDescription(code, description), ErrorType.Unauthorized);

    public static Error Forbidden(string code, string? description = null) =>
        new(code, GetDescription(code, description), ErrorType.Forbidden);

    public static Error ValidationSummary(string code, string? description = null, IReadOnlyList<Error>? errors = null) =>
        new(code, GetDescription(code, description), ErrorType.Validation, errors);
}
