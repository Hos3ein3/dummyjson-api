using System;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace DummyJson.Infrastructure.Caching;

public class HybridCacheService : IHybridCacheService
{
    #pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates.
    private readonly HybridCache _hybridCache;

    public HybridCacheService(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = expiration.HasValue
            ? new HybridCacheEntryOptions { Expiration = expiration.Value }
            : null;

        return await _hybridCache.GetOrCreateAsync(
            key,
            async cancel => await factory(cancel),
            options,
            cancellationToken: cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _hybridCache.RemoveAsync(key, cancellationToken);
    }
    #pragma warning restore EXTEXP0018
}
