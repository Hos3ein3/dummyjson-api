using Asp.Versioning;
using DummyJson.API.Endpoints;
using DummyJson.API.Middleware;
using DummyJson.Application;
using DummyJson.Infrastructure;
using DummyJson.Persistence;
using DummyJson.Persistence.Seeding;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Hangfire;
using Scalar.AspNetCore;
using Serilog;
using System.Threading.RateLimiting;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Interfaces;
using tusdotnet.Stores;
using System.IO;
using DummyJson.API.Extensions;

// ── Serilog Bootstrap Logger ──────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog Full Configuration ────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    // ── Layers ────────────────────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);

    // ── Feature Management ────────────────────────────────────────────────────
    builder.Services.AddFeatureManagement();

    // ── API Versioning ────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // ── Controllers (Removed in favor of Minimal APIs) ────────────────────────
    
    // ── Fluent Validation ─────────────────────────────────────────────────────
    builder.Services.AddValidatorsFromAssemblyContaining<DummyJson.Application.Products.Commands.CreateProductCommandValidator>();

    // ── OpenAPI / Scalar ──────────────────────────────────────────────────────
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "DummyJson API";
            document.Info.Description = "Clean Architecture + DDD backend for DummyJSON data.";
            document.Info.Version = "v1";
            return Task.CompletedTask;
        });
    });

    builder.Services.AddOpenApi("v2", options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "DummyJson API";
            document.Info.Description = "Clean Architecture + DDD backend for DummyJSON data.";
            document.Info.Version = "v2";
            return Task.CompletedTask;
        });
    });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["http://localhost:4200"];

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // ── HTTP Context & Exceptions ─────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddLocalization();

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();
        // .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL") ?? "", name: "PostgreSQL")
        // .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "", name: "Redis");
    
    if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHealthChecksUIAutoRequest"))
    {
        builder.Services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(15);
            options.MaximumHistoryEntriesPerEndpoint(60);
            options.SetApiMaxActiveRequests(1);
            options.AddHealthCheckEndpoint("API Health", "/health");
        }).AddInMemoryStorage();
    }

    // ── Rate Limiting ─────────────────────────────────────────────────────────
    builder.Services.AddCustomRateLimiting();

    // ── Resilience & Refit ────────────────────────────────────────────────────
    builder.Services.AddCustomPollyPipelines();
    builder.Services.AddCustomRefitClients();

    // ── Caching (Redis + Hybrid L1/L2) ─────────────────────────────────────────
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        options.InstanceName = "DummyJson_";
    });

    // .NET 9 HybridCache (Memory L1 + Redis L2)
    #pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates.
    builder.Services.AddHybridCache(options =>
    {
        options.MaximumPayloadBytes = 1024 * 1024;
        options.MaximumKeyLength = 1024;
        options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
            LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };
    });
    #pragma warning restore EXTEXP0018

    var app = builder.Build();

    // ── Log Connection Strings ────────────────────────────────────────────────
    Log.Information("Connection Strings:");
    Log.Information("  SQL (PostgreSQL): {Sql}", app.Configuration.GetConnectionString("PostgreSQL"));
    Log.Information("  SQL (SqlServer): {Sql}", app.Configuration.GetConnectionString("SqlServer"));
    Log.Information("  MongoDB: {Mongo}", app.Configuration.GetConnectionString("MongoDB"));
    Log.Information("  Redis: {Redis}", app.Configuration.GetConnectionString("Redis"));

    // ── Seed Data ─────────────────────────────────────────────────────────────
    if (app.Configuration.GetValue<bool>("SeedDatabase"))
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

        var seedPath = app.Configuration.GetValue<string>("SeedDataPath")
            ?? Path.Combine(Directory.GetCurrentDirectory(), "SeedData");

        // Resolve wwwroot — falls back gracefully if WebRootPath is not set
        var wwwRoot = app.Environment.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var seedImages = app.Configuration.GetValue<bool>("SeedImages");

        await seeder.SeedAsync(seedPath, wwwRoot, seedImages);
    }

    // ── Pipeline ──────────────────────────────────────────────────────────────
    app.UseExceptionHandler(); // Maps to IExceptionHandler / ProblemDetails
    app.UseMiddleware<RequestContextMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // For development/sample, allow all requests to see the dashboard
        Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() } 
    });

    // ── Tusdotnet Configuration ───────────────────────────────────────────────
    
    var tusUploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tus");
    if (!Directory.Exists(tusUploadDirectory))
        Directory.CreateDirectory(tusUploadDirectory);

    app.UseTus(context => new DefaultTusConfiguration
    {
        Store = new TusDiskStore(tusUploadDirectory),
        UrlPath = "/api/v1/files/tus",
        Events = new Events
        {
            OnBeforeCreateAsync = ctx =>
            {
                // Example Validation
                if (ctx.Metadata.ContainsKey("filetype"))
                {
                    var filetype = ctx.Metadata["filetype"].GetString(System.Text.Encoding.UTF8);
                    if (filetype != "image/jpeg" && filetype != "image/png" && filetype != "application/pdf" && filetype != "video/mp4")
                    {
                        ctx.FailRequest($"MimeType {filetype} is not allowed.");
                    }
                }
                return Task.CompletedTask;
            }
        }
    });


    // if (app.Environment.IsDevelopment())
    // {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("DummyJson API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("Bearer");
        });

        // Map separate Scalar endpoints for each version
        app.MapScalarApiReference("scalar-v1", options =>
            options.WithTitle("DummyJson API v1").WithOpenApiRoutePattern("/openapi/v1.json"));
        app.MapScalarApiReference("scalar-v2", options =>
            options.WithTitle("DummyJson API v2").WithOpenApiRoutePattern("/openapi/v2.json"));
    //}

    app.MapAuthEndpoints();
    app.MapProductEndpoints();
    app.MapCartsEndpoints();
    app.MapPostsEndpoints();
    app.MapTodosEndpoints();
    app.MapUsersEndpoints();
    app.MapSampleEndpoints();
    app.MapCacheSampleEndpoints();
    app.MapBackgroundJobsEndpoints();
    app.MapUtilitySamplesEndpoints();
    app.MapFileEndpoints();
    app.MapMediaProcessingEndpoints();

    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
    
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    if (app.Configuration.GetValue<bool>("FeatureManagement:EnableHealthChecksUIAutoRequest"))
    {
        app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
    }

    Log.Information("DummyJson API started. Scalar UI: https://localhost:*/scalar");
    Log.Information("env:{0}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
