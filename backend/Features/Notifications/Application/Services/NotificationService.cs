using backend.extensions.Services.Realtime.Abstractions;
using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces;
using backend.Features.Notifications.Domain.Entities;
using backend.Features.Notifications.Domain.Interfaces;

namespace backend.Features.Notifications.Application.Services;

public class NotificationService(
    INotificationRepository repository,
    IRealtimeService realtimeService) : INotificationService
{
    public async Task<NotificationSummaryDTO> GetUserNotificationsAsync(Guid userId)
    {
        // 1. Busca as últimas notificações (direto do banco)
        var notifications = await repository.GetActiveNotificationsByUserIdAsync(userId);

        // 2. Busca o contador de não lidas (provavelmente virá rápido do Redis)
        var unreadCount = await repository.GetUnreadCountAsync(userId);

        return new NotificationSummaryDTO(notifications, unreadCount);
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type)
    {
        // 1. Instancia a entidade de domínio
        var notification = new Notification(userId, type, title, message);

        // 2. Persiste no banco de dados e limpa o cache Redis
        await repository.SaveAsync(notification);

        // 3. Monta o DTO para enviar via WebSocket
        var dto = new NotificationResponseDTO(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.IsRead,
            notification.CreatedAt
        );

        // 4. Emite o evento em tempo real para o Frontend e App
        await realtimeService.PublishToUserAsync(userId.ToString(), "new_notification", dto);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        await repository.MarkAsReadAsync(notificationId, userId);
    }
}