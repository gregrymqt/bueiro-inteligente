using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Domain.Entities;
using backend.Features.Notifications.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Notifications.Infrastructure.Persistence.Repositories;

public class NotificationRepository(
    AppDbContext dbContext) : INotificationRepository
{
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
        return await dbContext.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task SaveAsync(Notification notification)
    {
        await dbContext.Notifications.AddAsync(notification);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        await dbContext.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        // EF Core 8: Bulk update direto no banco (MUITO mais rápido que fazer foreach)
        await dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}