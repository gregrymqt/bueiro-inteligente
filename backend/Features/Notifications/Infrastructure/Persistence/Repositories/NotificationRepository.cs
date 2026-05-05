using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Domain.Entities;
using backend.Features.Notifications.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Notifications.Infrastructure.Persistence.Repositories;

public class NotificationRepository(
    AppDbContext dbContext,
    ICacheService cacheService) : INotificationRepository
{
    // Chave de cache única por usuário
    private string GetUnreadCountCacheKey(Guid userId) => $"notifications:unread_count:{userId}";

    public async Task<IEnumerable<NotificationResponseDTO>> GetActiveNotificationsByUserIdAsync(Guid userId)
    {
        // Retornamos direto como DTO com AsNoTracking() para máxima performance
        return await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Evita payload gigante na tela inicial
            .Select(n => new NotificationResponseDTO(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var cacheKey = GetUnreadCountCacheKey(userId);

        // Usamos o GetOrSetAsync do ICacheService que você criou
        var response = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await dbContext.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
            TimeSpan.FromMinutes(30) // Expira em 30 minutos por segurança
        );

        return response.Data;
    }

    public async Task SaveAsync(Notification notification)
    {
        await dbContext.Notifications.AddAsync(notification);
        await dbContext.SaveChangesAsync();

        // Invalida o cache do contador após salvar uma nova notificação
        await cacheService.RemoveAsync(GetUnreadCountCacheKey(notification.UserId));
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is not null)
        {
            notification.MarkAsRead();
            await dbContext.SaveChangesAsync();

            // Invalida o cache
            await cacheService.RemoveAsync(GetUnreadCountCacheKey(userId));
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        // EF Core 8: Bulk update direto no banco (MUITO mais rápido que fazer foreach)
        await dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        // Invalida o cache
        await cacheService.RemoveAsync(GetUnreadCountCacheKey(userId));
    }
}