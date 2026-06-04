using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace DummyJson.API.Extensions;

public static class RateLimiterExtensions
{
    public enum RateLimiterPolicy
    {
        StrictPolicy,SlowEndpointPolicy,ActionLimiter
    }
    public static string GetRateLimiterPolicy(RateLimiterPolicy policy)
    {
        switch (policy)
        {
            case RateLimiterPolicy.StrictPolicy:
            default:
                return "StrictPolicy";
                break;
            case RateLimiterPolicy.SlowEndpointPolicy:
                return  "SlowEndpointPolicy";
                break;
            case RateLimiterPolicy.ActionLimiter:
                return  "ActionLimiter";
                break;
            
        }
    }
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddFixedWindowLimiter("StrictPolicy", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromSeconds(10);
                opt.QueueLimit = 0;
            });

            options.AddPolicy("SlowEndpointPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 1,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(5)
                    }));

            options.AddPolicy("ActionLimiter", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
