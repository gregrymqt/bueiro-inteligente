using backend.Features.Subscription.Domain.Entities;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Cache;

namespace backend.Features.Subscription.Infrastructure.Persistence.Repositories;

public sealed class CachedSubscriptionRepository(
    ISubscriptionRepository decorated,
    ICacheService cache,
    ILogger<CachedSubscriptionRepository> logger) : ISubscriptionRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    private static string GetCacheKeyId(Guid id) => $"subscription:id:{id}";
    private static string GetCacheKeyExternalId(string externalId) => $"subscription:ext_id:{externalId}";
    private static string GetCacheKeyUserId(Guid userId) => $"subscription:user_id:{userId}";

    public async Task<UserSubscription?> GetByIdAsync(Guid id)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync(
                GetCacheKeyId(id),
                () => decorated.GetByIdAsync(id),
                CacheTtl
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no cache para Subscription GetByIdAsync. Indo ao banco.");
            return await decorated.GetByIdAsync(id).ConfigureAwait(false);
        }
    }

    public async Task<UserSubscription?> GetByExternalIdAsync(string externalId)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync(
                GetCacheKeyExternalId(externalId),
                () => decorated.GetByExternalIdAsync(externalId),
                CacheTtl // Expira em meia hora
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no cache para Subscription GetByExternalIdAsync. Indo ao banco.");
            return await decorated.GetByExternalIdAsync(externalId).ConfigureAwait(false);
        }
    }

    public async Task<UserSubscription?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync(
                GetCacheKeyUserId(userId),
                () => decorated.GetByUserIdAsync(userId),
                CacheTtl
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no cache para Subscription GetByUserIdAsync. Indo ao banco.");
            return await decorated.GetByUserIdAsync(userId).ConfigureAwait(false);
        }
    }

    public async Task<UserSubscription> CreateAsync(UserSubscription subscription)
    {
        var result = await decorated.CreateAsync(subscription).ConfigureAwait(false);
        await InvalidateCacheAsync(result);
        return result;
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        await InvalidateCacheAsync(subscription);
        await decorated.UpdateAsync(subscription).ConfigureAwait(false);
    }

    private async Task InvalidateCacheAsync(UserSubscription subscription)
    {
        try
        {
            await cache.RemoveAsync(GetCacheKeyId(subscription.Id));
            await cache.RemoveAsync(GetCacheKeyUserId(subscription.UserId));
            
            if (!string.IsNullOrEmpty(subscription.ExternalId))
            {
                await cache.RemoveAsync(GetCacheKeyExternalId(subscription.ExternalId));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao invalidar cache para Subscription.");
        }
    }
}