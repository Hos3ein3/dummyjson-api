using System;
using System.Threading;
using System.Threading.Tasks;

namespace DummyJson.Application.Common.Interfaces.Caching;

public interface IHybridCacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
