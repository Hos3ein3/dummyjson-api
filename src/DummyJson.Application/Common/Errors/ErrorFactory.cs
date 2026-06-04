using Microsoft.Extensions.Localization;
using SharedKernel.Results;

namespace Application.Common.Errors;

/// <summary>
/// Implementation of IErrorFactory that uses IStringLocalizer to resolve descriptions.
/// </summary>
public class ErrorFactory(IStringLocalizer<ErrorMessages> localizer) : IErrorFactory
{
    public Error Failure(string code, params object[] args) =>
        Error.Failure(code, GetMessage(code, args));

    public Error Validation(string code, params object[] args) =>
        Error.Validation(code, GetMessage(code, args));

    public Error NotFound(string code, params object[] args) =>
        Error.NotFound(code, GetMessage(code, args));

    public Error Conflict(string code, params object[] args) =>
        Error.Conflict(code, GetMessage(code, args));

    public Error Unauthorized(string code, params object[] args) =>
        Error.Unauthorized(code, GetMessage(code, args));

    public Error Forbidden(string code, params object[] args) =>
        Error.Forbidden(code, GetMessage(code, args));

    private string GetMessage(string code, object[] args)
    {
        var localizedString = localizer[code, args];
        return localizedString.ResourceNotFound ? code : localizedString.Value;
    }
}
