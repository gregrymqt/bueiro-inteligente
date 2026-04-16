namespace backend.Extensions.Security.Infrastructure;

using backend.Extensions.Security.Abstractions;
using backend.Infrastructure.Cache;

public sealed class RedisRateLimitStore(ICacheService cacheService) : IRateLimitStore
{
    public async Task<int?> GetCountAsync(string key, CancellationToken ct = default) =>
        await cacheService.GetAsync<int?>(key);

    public async Task IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        var current = await cacheService.GetAsync<int>(key);
        await cacheService.SetAsync(key, current + 1, ttl);
    }
}
