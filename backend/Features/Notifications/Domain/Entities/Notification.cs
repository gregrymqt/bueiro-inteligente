using backend.Features.Notifications.Application.DTOs;

namespace backend.Features.Notifications.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Construtor vazio exigido pelo EF Core
    protected Notification()
    {
    }

    public Notification(Guid userId, NotificationType type, string title, string message)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
        }
    }
}