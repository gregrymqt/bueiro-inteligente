using backend.Core;
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
    ) =>
        await ExecuteAsync(async () =>
        {
            // Validação de segurança específica do IoT
            _authExtension.VerifyHardwareToken(
                Request.Headers.Authorization.ToString(),
                Request.Query["token"].ToString()
            );

            _logger.LogInformation("Recebendo medição do bueiro {Id}", payload.IdBueiro);

            var result = await _monitoringService
                .ProcessSensorDataAsync(payload, ct)
                .ConfigureAwait(false);
            return Ok(result);
        });

    [HttpGet("{id}/status")]
    public async Task<ActionResult<DrainStatusDTO>> GetStatus(string id, CancellationToken ct) =>
        await ExecuteAsync(async () =>
            Ok(await _monitoringService.GetDrainStatusAsync(id, ct).ConfigureAwait(false))
        );

    #region Helpers Enxutos

    private async Task<ActionResult> ExecuteAsync(Func<Task<ActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(CreateProblem("Unauthorized", ex.Message, 403));
        }
        catch (NotFoundException ex)
        {
            return NotFound(CreateProblem("Not found", ex.Message, 404));
        }
        catch (ConnectionException ex)
        {
            return StatusCode(503, CreateProblem("Connection error", ex.Message, 503));
        }
        catch (LogicException ex)
        {
            return BadRequest(CreateProblem("Validation error", ex.Message, 400));
        }
    }

    private static ProblemDetails CreateProblem(string title, string detail, int status) =>
        new()
        {
            Title = title,
            Detail = detail,
            Status = status,
        };

    #endregion
}
