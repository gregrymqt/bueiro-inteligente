using System.Security.Claims;
using backend.Features.Auth.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Users.Presentation.Controllers;

public sealed class DevicesController(IPushNotificationService pushNotificationService) : ApiControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> RegisterToken(
        [FromBody] RegisterDeviceRequest request,
        CancellationToken ct = default)
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(rawUserId, out var userId)) return Unauthorized();

        await pushNotificationService.RegisterDeviceAsync(userId, request.FcmToken, ct).ConfigureAwait(false);

        return NoContent();
    }
}