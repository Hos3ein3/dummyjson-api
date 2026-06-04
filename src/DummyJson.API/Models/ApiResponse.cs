namespace DummyJson.API.Models;

/// <summary>
/// Unified success envelope returned by every endpoint.
/// Frontend developers always receive the same top-level shape regardless
/// of which resource or operation was invoked.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
/// <remarks>
/// Success shape:
/// <code>
/// {
///   "success": true,
///   "statusCode": 200,
///   "data": { ... },
///   "meta": { "timestamp": "...", "traceId": "..." }
/// }
/// </code>
/// </remarks>
public sealed record ApiResponse<T>
{
    /// <summary>Always <c>true</c> for success responses.</summary>
    public bool Success { get; init; } = true;

    /// <summary>HTTP status code mirrored in the body for convenience.</summary>
    public int StatusCode { get; init; }

    /// <summary>The resource data. <c>null</c> for 204 No Content responses.</summary>
    public T? Data { get; init; }

    /// <summary>Optional request metadata (pagination, tracing, …).</summary>
    public ResponseMeta Meta { get; init; } = default!;
}

/// <summary>
/// Error envelope returned by every failing endpoint.
/// </summary>
/// <remarks>
/// Error shape:
/// <code>
/// {
///   "success": false,
///   "statusCode": 400,
///   "error": {
///     "code": "Validation",
///     "message": "...",
///     "errors": [ { "field": "email", "message": "..." } ]
///   },
///   "meta": { "timestamp": "...", "traceId": "...", "instance": "/api/v1/..." }
/// }
/// </code>
/// </remarks>
public sealed record ApiErrorResponse
{
    /// <summary>Always <c>false</c> for error responses.</summary>
    public bool Success { get; init; } = false;

    /// <summary>HTTP status code mirrored in the body.</summary>
    public int StatusCode { get; init; }

    /// <summary>Structured error detail.</summary>
    public ErrorDetail Error { get; init; } = default!;

    /// <summary>Request metadata.</summary>
    public ResponseMeta Meta { get; init; } = default!;
}

/// <summary>Structured error detail inside <see cref="ApiErrorResponse"/>.</summary>
public sealed record ErrorDetail
{
    /// <summary>Machine-readable error code (e.g. <c>"Validation"</c>, <c>"NotFound"</c>).</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-readable description of the error.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Field-level validation errors.
    /// <c>null</c> / empty when not a validation failure.
    /// </summary>
    public IReadOnlyList<FieldError>? Errors { get; init; }
}

/// <summary>Single field-level validation error.</summary>
public sealed record FieldError(string Field, string Message);

/// <summary>Metadata attached to every API response.</summary>
public sealed record ResponseMeta
{
    /// <summary>UTC timestamp of when the response was produced.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>ASP.NET Core trace identifier for log correlation.</summary>
    public string? TraceId { get; init; }

    /// <summary>OpenTelemetry / Activity trace id (when available).</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Request path — populated on error responses.</summary>
    public string? Instance { get; init; }

    /// <summary>
    /// Pagination information — populated when the data is a paged collection.
    /// </summary>
    public PaginationMeta? Pagination { get; init; }
}

/// <summary>Pagination metadata embedded in <see cref="ResponseMeta"/>.</summary>
public sealed record PaginationMeta(int Page, int PageSize, int TotalCount, int TotalPages)
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
