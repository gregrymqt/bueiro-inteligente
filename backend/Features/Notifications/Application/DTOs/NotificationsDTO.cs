namespace backend.Features.Notifications.Application.DTOs;

public enum NotificationType
{
    Info,
    Success,
    Error,
    Warning
}

public record NotificationResponseDTO(
    Guid Id,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTime CreatedAt
);

public record NotificationSummaryDTO(
    IEnumerable<NotificationResponseDTO> Notifications,
    int UnreadCount
);