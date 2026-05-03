using backend.extensions.Services.Security.Exceptions;
using backend.extensions.Services.Security.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Presentation.Hubs;

public sealed class MonitoringHub(WebSocketRateLimiter rateLimiter, ILogger<MonitoringHub> logger)
    : Hub
{
    private readonly WebSocketRateLimiter _rateLimiter =
        rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
    private readonly ILogger<MonitoringHub> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();

        if (httpContext is not null)
        {
            try
            {
                // Proteção contra excesso de conexões simultâneas
                await _rateLimiter
                    .EnforceAsync(httpContext, Context.ConnectionAborted)
                    .ConfigureAwait(false);
            }
            catch (RateLimitExceededException ex)
            {
                _logger.LogWarning(ex, "WebSocket rate limit: {Id}", Context.ConnectionId);
                throw new HubException(ex.Message);
            }
        }

        _logger.LogInformation("WebSocket connected: {Id}", Context.ConnectionId);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (ex is null)
            _logger.LogInformation("WebSocket closed: {Id}", Context.ConnectionId);
        else
            _logger.LogWarning(ex, "WebSocket closed with error: {Id}", Context.ConnectionId);

        await base.OnDisconnectedAsync(ex).ConfigureAwait(false);
    }
}
