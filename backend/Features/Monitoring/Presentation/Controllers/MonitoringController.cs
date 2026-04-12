using backend.Core;
using backend.Extensions;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace backend.Features.Monitoring.Presentation.Controllers;

[ApiController]
[Route("monitoring")]
public sealed class MonitoringController(
    IMonitoringService monitoringService,
    IAuthExtension authExtension,
    IRateLimiter rateLimiter,
    ILogger<MonitoringController> logger
) : ControllerBase
{
    private readonly IMonitoringService _monitoringService =
        monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));

    private readonly IAuthExtension _authExtension =
        authExtension ?? throw new ArgumentNullException(nameof(authExtension));

    private readonly IRateLimiter _rateLimiter =
        rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

    private readonly ILogger<MonitoringController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost("medicoes")]
    [AllowAnonymous]
    public async Task<ActionResult<DrainStatusDTO>> ReceiveSensorData(
        [FromBody] SensorPayloadDTO payload,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _rateLimiter.EnforceAsync(HttpContext, cancellationToken).ConfigureAwait(false);

            _authExtension.VerifyHardwareToken(
                Request.Headers.Authorization.ToString(),
                Request.Query["token"].ToString()
            );

            _logger.LogInformation(
                "Recepção de medição iniciada para o bueiro {DrainIdentifier}.",
                payload.IdBueiro
            );

            DrainStatusDTO result = await _monitoringService
                .ProcessSensorDataAsync(payload, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Recepção de medição concluída para o bueiro {DrainIdentifier}.",
                result.IdBueiro
            );

            return Ok(result);
        }
        catch (UnauthorizedAccessException exception)
        {
            return CreateProblem(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message
            );
        }
        catch (RateLimitExceededException exception)
        {
            return CreateTooManyRequestsProblem(exception);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    [HttpGet("{id}/status")]
    [Authorize(Roles = "User,Admin,Manager")]
    public async Task<ActionResult<DrainStatusDTO>> GetStatus(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _rateLimiter.EnforceAsync(HttpContext, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Solicitação de status recebida para o bueiro {DrainIdentifier}.", id);

            DrainStatusDTO result = await _monitoringService
                .GetDrainStatusAsync(id, cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
        catch (RateLimitExceededException exception)
        {
            return CreateTooManyRequestsProblem(exception);
        }
        catch (NotFoundException exception)
        {
            return CreateProblem(StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    private ObjectResult CreateTooManyRequestsProblem(RateLimitExceededException exception)
    {
        if (exception.RetryAfter.HasValue)
        {
            Response.Headers[HeaderNames.RetryAfter] =
                ((int)exception.RetryAfter.Value.TotalSeconds).ToString();
        }

        return CreateProblem(
            StatusCodes.Status429TooManyRequests,
            "Too many requests",
            exception.Message
        );
    }

    private static ObjectResult CreateProblem(int statusCode, string title, string detail)
    {
        return new ObjectResult(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
        })
        {
            StatusCode = statusCode,
        };
    }
}