using backend.extensions.Services.Security.Exceptions;
using backend.extensions.Services.Security.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Presentation.Hubs;

public sealed class ApplicationHub(WebSocketRateLimiter rateLimiter, ILogger<ApplicationHub> logger)
    : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            try 
            { 
                await rateLimiter.EnforceAsync(httpContext, Context.ConnectionAborted).ConfigureAwait(false); 
            }
            catch (RateLimitExceededException ex) 
            { 
                throw new HubException(ex.Message); 
            }
        }

        logger.LogInformation("Cliente conectado ao Hub Global: {Id}", Context.ConnectionId);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (ex is null)
            logger.LogInformation("WebSocket closed: {Id}", Context.ConnectionId);
        else
            logger.LogWarning(ex, "WebSocket closed with error: {Id}", Context.ConnectionId);

        await base.OnDisconnectedAsync(ex).ConfigureAwait(false);
    }
}
