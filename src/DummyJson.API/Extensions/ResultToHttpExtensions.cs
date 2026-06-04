using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using SharedKernel.Results;

namespace Api.Extensions;

/// <summary>
/// Extensions to convert Result objects into ASP.NET Core IResult responses.
/// </summary>
public static class ResultToHttpExtensions
{
    public static IResult ToIResult(this Result result, HttpContext context) =>
        result.IsSuccess ? Results.NoContent() : CreateProblemDetails(result.Error, context);

    public static IResult ToIResult<T>(this Result<T> result, HttpContext context) =>
        result.IsSuccess ? Results.Ok(result.Value) : CreateProblemDetails(result.Error, context);

    public static IResult ToIResult<T>(this Result<T> result, HttpContext context, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : CreateProblemDetails(result.Error, context);

    public static IResult ToCreatedIResult<T>(this Result<T> result, HttpContext context, string location) =>
        result.IsSuccess ? Results.Created(location, result.Value) : CreateProblemDetails(result.Error, context);

    public static IResult ToNoContentIResult(this Result result, HttpContext context) =>
        result.IsSuccess ? Results.NoContent() : CreateProblemDetails(result.Error, context);

    private static IResult CreateProblemDetails(Error error, HttpContext context)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var extensions = new Dictionary<string, object?>
        {
            { "code", error.Code },
            { "traceId", context.TraceIdentifier }
        };

        if (Activity.Current?.TraceId.ToString() is { } correlationId)
        {
            extensions.Add("correlationId", correlationId);
        }

        if (error.Errors is { Count: > 0 } nestedErrors)
        {
            extensions.Add("errors", nestedErrors);
        }

        return Results.Problem(
            title: error.Type.ToString(),
            detail: error.Description,
            statusCode: statusCode,
            instance: context.Request.Path,
            extensions: extensions);
    }
}