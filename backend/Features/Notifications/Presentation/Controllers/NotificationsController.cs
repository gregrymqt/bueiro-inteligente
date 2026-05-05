using System.Security.Claims;
using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Notifications.Presentation.Controllers;

public class NotificationsController(INotificationService notificationService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationSummaryDTO>> GetMyNotifications()
    {
        var userId = GetUserIdFromClaims();
        var result = await notificationService.GetUserNotificationsAsync(userId);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetUserIdFromClaims();
        await notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    private Guid GetUserIdFromClaims()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(id!);
    }
}