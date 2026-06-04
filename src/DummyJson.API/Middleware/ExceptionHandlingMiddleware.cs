using System.Net;
using System.Text.Json;
using SharedKernel.Results;
using Microsoft.AspNetCore.Mvc;

namespace DummyJson.API.Middleware;

/// <summary>
/// Global exception handler — converts exceptions into RFC 7807 ProblemDetails responses.
/// Prevents stack traces from leaking to clients.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
            ArgumentException e => (HttpStatusCode.BadRequest, "Bad Request", e.Message),
            InvalidOperationException e => (HttpStatusCode.BadRequest, "Invalid Operation", e.Message),
            KeyNotFoundException e => (HttpStatusCode.NotFound, "Not Found", e.Message),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "Request Cancelled", "The request was cancelled."),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
