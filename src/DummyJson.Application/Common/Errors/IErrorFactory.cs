using SharedKernel.Results;

namespace Application.Common.Errors;

/// <summary>
/// Defines a factory for creating localized errors.
/// </summary>
public interface IErrorFactory
{
    Error Failure(string code, params object[] args);
    Error Validation(string code, params object[] args);
    Error NotFound(string code, params object[] args);
    Error Conflict(string code, params object[] args);
    Error Unauthorized(string code, params object[] args);
    Error Forbidden(string code, params object[] args);
}
