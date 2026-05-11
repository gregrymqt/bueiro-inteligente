using backend.Features.Drains.Domain.Entities;
using backend.Features.Drains.Domain.Interfaces;
using backend.Infrastructure.Cache;

namespace backend.Features.Drains.Infrastructure.Persistence.Repositories;

public sealed class CachedDrainRepository(IDrainRepository decorated, ICacheService cache)
    : IDrainRepository
{
    private const string CacheKeyAllDrains = "drains:all";
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(30);

    private static string GetDrainCacheKey(Guid id) => $"drains:id:{id}";

    private static string GetDrainHardwareCacheKey(string hardwareId) => $"drains:hw:{hardwareId}";

    public async Task<Drain?> GetByIdAsync(Guid drainId, CancellationToken ct = default)
    {
        var cacheResult = await cache.GetOrSetAsync(
            GetDrainCacheKey(drainId),
            async () => await decorated.GetByIdAsync(drainId, ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<Drain?> GetByHardwareIdAsync(
        string hardwareId,
        CancellationToken ct = default
    )
    {
        var cacheResult = await cache.GetOrSetAsync(
            GetDrainHardwareCacheKey(hardwareId),
            async () => await decorated.GetByHardwareIdAsync(hardwareId, ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<IReadOnlyList<Drain>> GetAllAsync(
        int skip = 0,
        int limit = 100,
        CancellationToken ct = default
    )
    {
        var cacheResult = await cache.GetOrSetAsync(
            $"{CacheKeyAllDrains}:s{skip}:l{limit}",
            async () => await decorated.GetAllAsync(skip, limit, ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<Drain> CreateAsync(Drain drain, CancellationToken ct = default)
    {
        var result = await decorated.CreateAsync(drain, ct).ConfigureAwait(false);
        await InvalidateCacheAsync(drain);
        return result;
    }

    public async Task<Drain> UpdateAsync(Drain drain, CancellationToken ct = default)
    {
        var result = await decorated.UpdateAsync(drain, ct).ConfigureAwait(false);
        await InvalidateCacheAsync(drain);
        return result;
    }

    public async Task DeleteAsync(Drain drain, CancellationToken ct = default)
    {
        await decorated.DeleteAsync(drain, ct).ConfigureAwait(false);
        await InvalidateCacheAsync(drain);
    }

    private async Task InvalidateCacheAsync(Drain drain)
    {
        await cache.RemoveAsync(GetDrainCacheKey(drain.Id));
        await cache.RemoveAsync(GetDrainHardwareCacheKey(drain.HardwareId));
        await cache.RemoveAsync($"{CacheKeyAllDrains}:s0:l100"); // Remove common list queries if needed
    }
}
