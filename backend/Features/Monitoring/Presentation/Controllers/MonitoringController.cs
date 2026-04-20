using backend.Extensions.Auth.Abstractions;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Monitoring.Presentation.Controllers;

[Authorize(Roles = "User,Admin,Manager")]
public sealed class MonitoringController(
    IMonitoringService monitoringService,
    IAuthExtension authExtension,
    ILogger<MonitoringController> logger
) : ApiControllerBase
{
    // C# 12: Campos injetados via Primary Constructor
    private readonly IMonitoringService _monitoringService =
        monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
    private readonly IAuthExtension _authExtension =
        authExtension ?? throw new ArgumentNullException(nameof(authExtension));
    private readonly ILogger<MonitoringController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost("medicoes")]
    [AllowAnonymous] // O hardware usa token próprio via Query ou Header
    public async Task<ActionResult<DrainStatusDTO>> ReceiveSensorData(
        [FromBody] SensorPayloadDTO payload,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(payload);

        _logger.LogInformation("Recebendo medição do bueiro {Id}", payload.IdBueiro);

        _authExtension.VerifyHardwareToken(
            Request.Headers.Authorization.ToString(),
            Request.Query["token"].ToString()
        );

        return Ok(
            await _monitoringService.ProcessSensorDataAsync(payload, ct).ConfigureAwait(false)
        );
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<DrainStatusDTO>> GetStatus(string id, CancellationToken ct) =>
        Ok(await _monitoringService.GetDrainStatusAsync(id, ct).ConfigureAwait(false));
}
