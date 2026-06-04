namespace SharedKernel.Results;

/// <summary>
/// Defines the types of errors that can occur.
/// </summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}
