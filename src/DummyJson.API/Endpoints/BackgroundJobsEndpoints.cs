using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DummyJson.API.Endpoints;

public static class BackgroundJobsEndpoints
{
    public static void MapBackgroundJobsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/jobs").WithTags("Background Jobs");

        // ── Hangfire ──────────────────────────────────────────────────────────
        
        group.MapPost("/hangfire/fire-and-forget", (IBackgroundJobClient backgroundJobs) =>
        {
            var jobId = backgroundJobs.Enqueue(() => Console.WriteLine($"[Hangfire] Fire-and-forget job executed at {DateTime.Now}"));
            return Results.Ok(new { JobId = jobId, Message = "Hangfire job enqueued! Check /hangfire dashboard." });
        });

        group.MapPost("/hangfire/delayed", (IBackgroundJobClient backgroundJobs) =>
        {
            var jobId = backgroundJobs.Schedule(() => Console.WriteLine($"[Hangfire] Delayed job executed at {DateTime.Now}"), TimeSpan.FromSeconds(10));
            return Results.Ok(new { JobId = jobId, Message = "Hangfire delayed job scheduled for 10 seconds from now!" });
        });

        // ── Quartz.NET ────────────────────────────────────────────────────────

        group.MapPost("/quartz/schedule", async (ISchedulerFactory schedulerFactory) =>
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<SampleQuartzJob>()
                .WithIdentity("sampleJob", "group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("sampleTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .WithRepeatCount(2)) // Runs exactly 3 times (first + 2 repeats)
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            return Results.Ok(new { Message = "Quartz job scheduled to run 3 times every 10 seconds!" });
        });

    }
}

// ── Quartz Sample Job ─────────────────────────────────────────────────────────

public class SampleQuartzJob : IJob
{
    private readonly ILogger<SampleQuartzJob> _logger;

    public SampleQuartzJob(ILogger<SampleQuartzJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[Quartz] Sample Job executed at {Time}", DateTime.Now);
        return Task.CompletedTask;
    }
}
