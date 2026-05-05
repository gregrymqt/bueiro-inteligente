using backend.Features.Notifications.Application.DTOs;

namespace backend.Features.Notifications.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationSummaryDTO> GetUserNotificationsAsync(Guid userId);
    
    // Método centralizado para ser usado por Jobs (Hangfire) e outros Services
    Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type);
    
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
}