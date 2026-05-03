using backend.Core;
using backend.Extensions.Security.Utils;
using backend.extensions.Services.Security.Abstractions;
using backend.extensions.Services.Security.Exceptions;

namespace backend.extensions.Services.Security.Infrastructure;

public sealed class RateLimiter(
    IRateLimitStore rateLimitStore,
    ILogger<RateLimiter> logger,
    int times = 5,
    int seconds = 10
) : IRateLimiter
{
    public int Times => times;
    public int Seconds => seconds;

    public async Task EnforceAsync(HttpContext context, CancellationToken ct = default)
    {
        _ = context ?? throw LogicException.NullValue(nameof(context));

        try
        {
            var key =
                $"rate_limit:{SecurityUtils.ResolveIdentifier(context)}:{context.Request.Path.Value ?? "/"}";
            var currentCount = await rateLimitStore.GetCountAsync(key, ct);

            if (currentCount >= times)
                throw new RateLimitExceededException(
                    "Limite de requisições excedido.",
                    TimeSpan.FromSeconds(seconds)
                );

            await rateLimitStore.IncrementAsync(key, TimeSpan.FromSeconds(seconds), ct);
        }
        catch (RateLimitExceededException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro no processamento do Rate Limit.");
        }
    }
}
