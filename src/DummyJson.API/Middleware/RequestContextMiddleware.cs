using Serilog.Context;

namespace DummyJson.API.Middleware;

/// <summary>
/// Middleware to extract or generate context IDs and push them into Serilog's LogContext.
/// Sets the CorrelationId, RequestId, TenantId, TraceId, and ClientId for downstream tracking.
/// </summary>
public sealed class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var requestId = context.Request.Headers["X-Request-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "default";
        var traceId = context.TraceIdentifier;
        var clientId = context.Request.Headers["X-Client-Id"].FirstOrDefault() ?? "unknown";

        context.Items["CorrelationId"] = correlationId;
        context.Items["RequestId"] = requestId;
        context.Items["TenantId"] = tenantId;
        context.Items["TraceId"] = traceId;
        context.Items["ClientId"] = clientId;

        // Optionally, append back to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
            {
                context.Response.Headers.Append("X-Correlation-Id", correlationId);
            }
            return Task.CompletedTask;
        });

        // Push properties to Serilog
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("ClientId", clientId))
        {
            await _next(context);
        }
    }
}
