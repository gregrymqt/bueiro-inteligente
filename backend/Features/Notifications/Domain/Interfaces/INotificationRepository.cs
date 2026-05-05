using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Domain.Entities;

namespace backend.Features.Notifications.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<NotificationResponseDTO>> GetActiveNotificationsByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task SaveAsync(Notification notification); // Entidade de domínio
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
}