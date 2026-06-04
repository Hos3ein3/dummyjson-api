using System;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Users;
using DummyJson.Application.Common.Interfaces.Caching;
using DummyJson.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class CacheSampleEndpoints
{
    public static void MapCacheSampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/cache-samples").WithTags("Hybrid Cache");

        group.MapGet("/in-memory/user-preferences/{userId:guid}", async (
            Guid userId,
            IInMemoryCacheService cacheService,
            IMongoRepository<UserPreferences> repository,
            CancellationToken ct) =>
        {
            var cacheKey = $"InMemory_UserPreferences:{userId}";

            var preferences = await cacheService.GetAsync<UserPreferences>(cacheKey, ct);
            if (preferences is null)
            {
                await Task.Delay(500, ct); // Simulate slow DB hit
                preferences = await repository.GetByIdAsync(userId, ct);
                
                if (preferences is not null)
                {
                    await cacheService.SetAsync(cacheKey, preferences, TimeSpan.FromMinutes(5), ct);
                }
            }

            return preferences is not null ? Results.Ok(preferences) : Results.NotFound();
        });

        group.MapGet("/redis/user-preferences/{userId:guid}", async (
            Guid userId,
            IRedisCacheService cacheService,
            IMongoRepository<UserPreferences> repository,
            CancellationToken ct) =>
        {
            var cacheKey = $"Redis_UserPreferences:{userId}";

            var preferences = await cacheService.GetAsync<UserPreferences>(cacheKey, ct);
            if (preferences is null)
            {
                await Task.Delay(500, ct); // Simulate slow DB hit
                preferences = await repository.GetByIdAsync(userId, ct);
                
                if (preferences is not null)
                {
                    await cacheService.SetAsync(cacheKey, preferences, TimeSpan.FromMinutes(5), ct);
                }
            }

            return preferences is not null ? Results.Ok(preferences) : Results.NotFound();
        });

        group.MapGet("/hybrid/user-preferences/{userId:guid}", async (
            Guid userId,
            IHybridCacheService cacheService,
            IMongoRepository<UserPreferences> repository,
            CancellationToken ct) =>
        {
            var cacheKey = $"Hybrid_UserPreferences:{userId}";

            var preferences = await cacheService.GetOrCreateAsync(
                cacheKey,
                async cancel => 
                {
                    await Task.Delay(500, cancel); 
                    var data = await repository.GetByIdAsync(userId, cancel);
                    // Handle potential null gracefully depending on domain, or return an empty preference object
                    return data ?? UserPreferences.Create(userId);
                },
                TimeSpan.FromMinutes(5),
                cancellationToken: ct);

            return Results.Ok(preferences);
        });
    }
}
