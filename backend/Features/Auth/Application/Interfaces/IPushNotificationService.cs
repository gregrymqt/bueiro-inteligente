using backend.Features.Drains.Domain.Entities;

namespace backend.Features.Notifications.Application.Interfaces;

public interface IPushNotificationService
{
    Task RegisterDeviceAsync(Guid userId, string fcmToken, CancellationToken ct = default);
    Task SendDrainAlertAsync(string status, double obstructionLevel, Drain bueiro, CancellationToken ct = default);
}