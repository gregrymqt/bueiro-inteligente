using backend.Core;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Configuration;
using backend.Features.Monitoring.Domain.Entities;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Infrastructure.Cache;

namespace backend.Features.Monitoring.Infrastructure.Persistence.Repositories;

public sealed class CachedMonitoringRepository(
    IMonitoringRepository decorated,
    ICacheService cache
) : IMonitoringRepository
{
    private const string CacheKeyPrefix = "bueiro:";
    private static readonly TimeSpan CurrentStatusCacheTtl = TimeSpan.FromHours(1);

    private static string GetStatusCacheKey(string drainId) => $"{CacheKeyPrefix}{drainId}:status";

    public async Task InsertAsync(DrainStatus entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await decorated.InsertAsync(entity, ct).ConfigureAwait(false);

        await cache
            .SetAsync(GetStatusCacheKey(entity.DrainIdentifier), MapToDto(entity), CurrentStatusCacheTtl)
            .ConfigureAwait(false);
    }

    public async Task<DrainStatusDTO?> GetLatestStatusAsync(
        string drainId,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(drainId))
            throw LogicException.InvalidValue(nameof(drainId), drainId);

        var cacheResult = await cache
            .GetOrSetAsync(
                GetStatusCacheKey(drainId),
                async () => await decorated.GetLatestStatusAsync(drainId, ct).ConfigureAwait(false),
                CurrentStatusCacheTtl
            )
            .ConfigureAwait(false);

        return cacheResult.Data;
    }

    public Task<IReadOnlyList<DrainStatusDTO>> GetUnsyncedDataAsync(
        int limit = 100,
        CancellationToken ct = default
    ) => decorated.GetUnsyncedDataAsync(limit, ct);

    public Task MarkAsSyncedAsync(IReadOnlyCollection<string> drainIds, CancellationToken ct = default) =>
        decorated.MarkAsSyncedAsync(drainIds, ct);

    public Task<BueiroConfiguration> GetConfigByIdAsync(string id, CancellationToken ct = default) =>
        decorated.GetConfigByIdAsync(id, ct);

    private static DrainStatusDTO MapToDto(DrainStatus entity) =>
        new(
            entity.DrainIdentifier,
            entity.DistanceCm,
            entity.ObstructionLevel,
            entity.Status,
            entity.Latitude,
            entity.Longitude,
            entity.LastUpdate,
            entity.DataHash
        );
}