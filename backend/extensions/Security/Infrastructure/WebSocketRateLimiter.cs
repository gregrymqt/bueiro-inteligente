using backend.Core;
using backend.Extensions.Security.Abstractions;
using backend.Extensions.Security.Exceptions;
using backend.Extensions.Security.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace backend.Extensions.Security.Infrastructure;

public sealed class WebSocketRateLimiter
{
    private readonly IRateLimitStore _rateLimitStore;
    private readonly ILogger<WebSocketRateLimiter> _logger;

    public WebSocketRateLimiter(
        IRateLimitStore rateLimitStore,
        ILogger<WebSocketRateLimiter> logger,
        int times = 5,
        int seconds = 10
    )
    {
        _rateLimitStore = rateLimitStore ?? throw new ArgumentNullException(nameof(rateLimitStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Times = times;
        Seconds = seconds;
    }

    public int Times { get; }

    public int Seconds { get; }

    public async Task EnforceAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (context is null)
        {
            throw LogicException.NullValue(nameof(context));
        }

        try
        {
            string identifier = SecurityUtils.ResolveIdentifier(context);
            string routePath = context.Request.Path.Value ?? "/";
            string key = $"ws_rate_limit:{identifier}:{routePath}";

            int? currentCount = await _rateLimitStore
                .GetCountAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (currentCount.HasValue && currentCount.Value >= Times)
            {
                throw new RateLimitExceededException(
                    "Conexão WebSocket bloqueada por limite de requisições.",
                    TimeSpan.FromSeconds(Seconds)
                );
            }

            await _rateLimitStore
                .IncrementAsync(key, TimeSpan.FromSeconds(Seconds), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RateLimitExceededException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro no validador do Redis (WebSocketRateLimiter).");
        }
    }

}
