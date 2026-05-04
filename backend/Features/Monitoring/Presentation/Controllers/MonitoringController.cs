using backend.extensions.Services.Auth.Abstractions;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Application.Interfaces;
using backend.Features.Monitoring.Application.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Monitoring.Presentation.Controllers;

[Authorize(Roles = "User,Admin,Manager")]
public sealed class MonitoringController(
    IMonitoringService monitoringService,
    IBackgroundJobClient backgroundJobs,
    IAuthExtension authExtension,
    ILogger<MonitoringController> logger
) : ApiControllerBase
{
    [HttpPost("medicoes")]
    [AllowAnonymous] // O hardware usa token próprio via Query ou Header
    public Task<IActionResult> ReceiveSensorData(
        [FromBody] SensorPayloadDTO payload,
        CancellationToken ct
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(payload);

            logger.LogInformation("Recebendo medição do bueiro {Id}", payload.IdBueiro);

            authExtension.VerifyHardwareToken(
                Request.Headers.Authorization.ToString(),
                Request.Query["token"].ToString()
            );

            try
            {
                backgroundJobs.Enqueue<IMonitoringService>(service =>
                    service.ProcessSensorDataAsync(payload, CancellationToken.None)
                );
            }
            catch (Exception ex)
            {
                logger.LogCritical(
                    ex,
                    "Falha ao enfileirar processamento (Redis Down) para o bueiro {IdBueiro}",
                    payload.IdBueiro
                );
                return Task.FromResult<IActionResult>(Problem(
                    detail: "Falha ao enfileirar processamento (Redis Down)",
                    statusCode: 500
                ));
            }

            return Task.FromResult<IActionResult>(Accepted());
        }
        catch (Exception exception)
        {
            return Task.FromException<IActionResult>(exception);
        }
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<DrainStatusDTO>> GetStatus(string id, CancellationToken ct) =>
        Ok(await monitoringService.GetDrainStatusAsync(id, ct).ConfigureAwait(false));
}
