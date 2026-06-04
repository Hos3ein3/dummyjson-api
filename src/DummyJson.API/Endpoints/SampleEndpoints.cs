using Api.Extensions;
using DummyJson.Application.Common.Dispatcher;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Samples.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;
using FluentValidation;
using Polly.Registry;

namespace DummyJson.API.Endpoints;

public static class SampleEndpoints
{
    public static void MapSampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/samples").WithTags("Samples");

        group.MapGet("/result-extensions", async () =>
        {
            // 1. Retry
            var result = await Result.RetryAsync(async () =>
            {
                var random = new Random();
                if (random.Next(2) == 0) return Result.Failure<string>(Error.Failure("Sample.Retry", "Random failure"));
                return Result.Success("Success after retry!");
            }, maxAttempts: 3, delay: TimeSpan.FromMilliseconds(100));

            // 2. Map and Ensure
            var mappedResult = result
                .Map(val => val.ToUpper())
                .Ensure(val => val.Contains("SUCCESS"), Error.Validation("Sample.Ensure", "Value must contain SUCCESS"));

            // 3. Tap
            mappedResult.Tap(val => Console.WriteLine($"Tapped value: {val}"));

            return mappedResult.IsSuccess ? Results.Ok(mappedResult.Value) : Results.BadRequest(mappedResult.Error);
        });

        group.MapPost("/command-normal", async (
            [FromBody] NormalCommand command,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync<NormalCommand, Result<Guid>>(command, ct);
            return result.ToIResult(context);
        });

        group.MapPost("/command-transactional", async (
            [FromBody] TransactionalCommand command,
            IDispatcher dispatcher,
            HttpContext context,
            CancellationToken ct) =>
        {
            try
            {
                var result = await dispatcher.SendAsync<TransactionalCommand, Result<Guid>>(command, ct);
                return result.ToIResult(context);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Transaction Rolled Back");
            }
        });

        group.MapGet("/refit", async (
            ISampleRefitClient client,
            CancellationToken ct) =>
        {
            var todo = await client.GetSampleTodoAsync(ct);
            return Results.Ok(todo);
        });

        // Demonstrating another Refit endpoint
        group.MapGet("/refit-post/{id}", async (
            int id,
            ISampleRefitClient client,
            CancellationToken ct) =>
        {
            var post = await client.GetPostAsync(id, ct);
            return Results.Ok(post);
        });

        // Demonstrating manual Polly usage with Retry
        group.MapGet("/polly/retry", async (
            ResiliencePipelineProvider<string> pipelineProvider,
            CancellationToken ct) =>
        {
            var pipeline = pipelineProvider.GetPipeline("default");
            int attempts = 0;
            var result = await pipeline.ExecuteAsync(async token =>
            {
                attempts++;
                if (attempts < 3) throw new InvalidOperationException($"Attempt {attempts} failed.");
                await Task.Delay(100, token);
                return $"Success on attempt {attempts}";
            }, ct);
            return Results.Ok(new { Message = result, TotalAttempts = attempts });
        });

        // Demonstrating manual Polly usage with Timeout
        group.MapGet("/polly/timeout", async (
            ResiliencePipelineProvider<string> pipelineProvider,
            CancellationToken ct) =>
        {
            var pipeline = pipelineProvider.GetPipeline("timeout");
            try
            {
                await pipeline.ExecuteAsync(async token =>
                {
                    // Simulating a long running process
                    await Task.Delay(TimeSpan.FromSeconds(3), token);
                }, ct);
                return Results.Ok("Finished in time!");
            }
            catch (Polly.Timeout.TimeoutRejectedException)
            {
                return Results.Problem(detail: "The operation timed out.", statusCode: StatusCodes.Status408RequestTimeout);
            }
        });

        // Demonstrating manual Polly usage with Circuit Breaker
        group.MapGet("/polly/circuit-breaker", async (
            ResiliencePipelineProvider<string> pipelineProvider,
            [FromQuery] bool triggerFailure,
            CancellationToken ct) =>
        {
            var pipeline = pipelineProvider.GetPipeline("circuit-breaker");
            try
            {
                var result = await pipeline.ExecuteAsync(async token =>
                {
                    if (triggerFailure) throw new InvalidOperationException("Triggering circuit breaker failure...");
                    await Task.Delay(10, token);
                    return "Operation succeeded!";
                }, ct);
                return Results.Ok(result);
            }
            catch (Polly.CircuitBreaker.BrokenCircuitException)
            {
                return Results.Problem(detail: "The circuit breaker is currently OPEN.", statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        // Demonstrating custom rate limiting
        group.MapGet("/rate-limited", () => Results.Ok("This endpoint is strictly rate limited!"))
            .RequireRateLimiting("StrictPolicy");

        group.MapGet("/rate-limited-slow", () => Results.Ok("You can only call this once every 5 minutes!"))
            .RequireRateLimiting("SlowEndpointPolicy");

        group.MapGet("/rate-limited-action", () => Results.Ok("You can call this action 10 times per minute!"))
            .RequireRateLimiting("ActionLimiter");

        // Demonstrating FluentValidation integration
        group.MapPost("/validate", async (
            [FromBody] SampleValidationRequest request,
            FluentValidation.IValidator<SampleValidationRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            return Results.Ok($"Valid request for {request.Email}");
        });
    }
}

public record SampleValidationRequest(string Email, int Age);

public class SampleValidationRequestValidator : FluentValidation.AbstractValidator<SampleValidationRequest>
{
    public SampleValidationRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18).WithMessage("Must be at least 18.");
    }
}
