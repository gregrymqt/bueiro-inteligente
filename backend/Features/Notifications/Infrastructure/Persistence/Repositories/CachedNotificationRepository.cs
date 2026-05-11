using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Domain.Entities;
using backend.Features.Notifications.Domain.Interfaces;
using backend.Infrastructure.Cache;

namespace backend.Features.Notifications.Infrastructure.Persistence.Repositories;

public sealed class CachedNotificationRepository(
    INotificationRepository decorated,
    ICacheService cacheService,
    ILogger<CachedNotificationRepository> logger) : INotificationRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    private static string GetUnreadCountCacheKey(Guid userId) => $"notifications:unread_count:{userId}";
    private static string GetActiveNotificationsCacheKey(Guid userId) => $"notifications:active:{userId}";

    public async Task<IEnumerable<NotificationResponseDTO>> GetActiveNotificationsByUserIdAsync(Guid userId)
    {
        try
        {
            var cacheResult = await cacheService.GetOrSetAsync(
                GetActiveNotificationsCacheKey(userId),
                () => decorated.GetActiveNotificationsByUserIdAsync(userId),
                CacheTtl
            );
            return cacheResult.Data ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro no cache ao buscar notificações ativas. Falha tolerada, buscando do banco.");
            return await decorated.GetActiveNotificationsByUserIdAsync(userId).ConfigureAwait(false);
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var cacheResult = await cacheService.GetOrSetAsync(
                GetUnreadCountCacheKey(userId),
                () => decorated.GetUnreadCountAsync(userId),
                CacheTtl
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro no cache ao buscar contagem não lida. Falha tolerada, buscando do banco.");
            return await decorated.GetUnreadCountAsync(userId).ConfigureAwait(false);
        }
    }

    public async Task SaveAsync(Notification notification)
    {
        await decorated.SaveAsync(notification).ConfigureAwait(false);

        try
        {
            await cacheService.RemoveAsync(GetUnreadCountCacheKey(notification.UserId));
            await cacheService.RemoveAsync(GetActiveNotificationsCacheKey(notification.UserId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao invalidar cache após salvar notificação.");
        }
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        await decorated.MarkAsReadAsync(notificationId, userId).ConfigureAwait(false);

        try
        {
            await cacheService.RemoveAsync(GetUnreadCountCacheKey(userId));
            await cacheService.RemoveAsync(GetActiveNotificationsCacheKey(userId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao invalidar cache após marcar notificação como lida.");
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await decorated.MarkAllAsReadAsync(userId).ConfigureAwait(false);

        try
        {
            await cacheService.RemoveAsync(GetUnreadCountCacheKey(userId));
            await cacheService.RemoveAsync(GetActiveNotificationsCacheKey(userId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao invalidar cache após marcar todas as notificações como lidas.");
        }
    }
}
