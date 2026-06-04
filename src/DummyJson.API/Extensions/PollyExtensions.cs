using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace DummyJson.API.Extensions;

public static class PollyExtensions
{
    public static IServiceCollection AddCustomPollyPipelines(this IServiceCollection services)
    {
        // 1. Default resilience pipeline (Retry + Timeout)
        services.AddResiliencePipeline("default", builder =>
        {
            builder
                .AddRetry(new Polly.Retry.RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    Delay = TimeSpan.FromSeconds(2),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                .AddTimeout(TimeSpan.FromSeconds(10));
        });

        // 2. Timeout-only pipeline
        services.AddResiliencePipeline("timeout", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(1));
        });

        // 3. Circuit Breaker pipeline
        services.AddResiliencePipeline("circuit-breaker", builder =>
        {
            builder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(5),
                MinimumThroughput = 2,
                BreakDuration = TimeSpan.FromSeconds(15)
            });
        });

        return services;
    }
}
