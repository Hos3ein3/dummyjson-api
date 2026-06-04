using DummyJson.Application.Auth.Services;
using DummyJson.Application.Common.Events;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Common.Interfaces.Caching;
using DummyJson.Infrastructure.Auth;
using DummyJson.Infrastructure.Caching;
using DummyJson.Infrastructure.Events;
using DummyJson.Infrastructure.Identity;
using DummyJson.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using Hangfire;
using Quartz;

namespace DummyJson.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── JWT Settings ──────────────────────────────────────────────────────
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings not configured.");
        services.AddSingleton(jwtSettings);

        // ── JWT Authentication ────────────────────────────────────────────────
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        })
        .AddGoogle(options =>
        {
            var googleAuthNSection = configuration.GetSection("Authentication:Google");
            options.ClientId = googleAuthNSection["ClientId"] ?? "placeholder-client-id";
            options.ClientSecret = googleAuthNSection["ClientSecret"] ?? "placeholder-client-secret";
        });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // ── Event Dispatchers ─────────────────────────────────────────────────
        services.AddScoped<IntegrationEventDispatcher>();
        
        // ── Infrastructure Services ───────────────────────────────────────────
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));
        
        services.AddScoped<IFileService, DummyJson.Infrastructure.Services.FileService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<ISmsService, SmsService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // ── Caching Services ──────────────────────────────────────────────────
        services.AddMemoryCache();
        services.AddSingleton<IInMemoryCacheService, InMemoryCacheService>();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IHybridCacheService, HybridCacheService>();

        // ── Background Jobs ───────────────────────────────────────────────────
        
        // 1. Hangfire
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());
        services.AddHangfireServer();

        // 2. Quartz.NET
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            // Just configuring the base Quartz here. Jobs can be scheduled directly or via endpoints.
        });
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
