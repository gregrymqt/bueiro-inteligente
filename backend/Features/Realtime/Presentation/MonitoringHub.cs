using backend.Extensions.Security.Exceptions;
using backend.Extensions.Security.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Presentation;

/// <summary>
/// SignalR hub used as the WebSocket entry point for realtime clients.
/// SignalR manages keep-alive ping/pong automatically.
/// </summary>
public sealed class MonitoringHub(WebSocketRateLimiter rateLimiter, ILogger<MonitoringHub> logger)
    : Hub
{
    private readonly WebSocketRateLimiter _rateLimiter =
        rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

    private readonly ILogger<MonitoringHub> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task OnConnectedAsync()
    {
        HttpContext? httpContext = Context.GetHttpContext();

        if (httpContext is not null)
        {
            try
            {
                await _rateLimiter
                    .EnforceAsync(httpContext, Context.ConnectionAborted)
                    .ConfigureAwait(false);
            }
            catch (RateLimitExceededException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Conexão WebSocket bloqueada por rate limit. ConnectionId={ConnectionId}",
                    Context.ConnectionId
                );

                throw new HubException(exception.Message);
            }
        }

        _logger.LogInformation(
            "Iniciando nova conexão WebSocket... ConnectionId={ConnectionId}",
            Context.ConnectionId
        );

        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
        {
            _logger.LogInformation(
                "Conexão WebSocket encerrada. ConnectionId={ConnectionId}",
                Context.ConnectionId
            );
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Conexão WebSocket encerrada com erro. ConnectionId={ConnectionId}",
                Context.ConnectionId
            );
        }

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}
