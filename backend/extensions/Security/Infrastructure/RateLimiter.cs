using System.Security.Claims;
using backend.Core;
using backend.Extensions.Security.Abstractions;
using backend.Extensions.Security.Exceptions;

namespace backend.Extensions.Security.Infrastructure;

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
                $"rate_limit:{ResolveIdentifier(context)}:{context.Request.Path.Value ?? "/"}";
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

    // Método simplificado e reaproveitável
    public static string ResolveIdentifier(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        if (context.User?.Identity?.IsAuthenticated == true)
            return context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue(ClaimTypes.Name)
                ?? "auth-user";

        return context.Items["user_id"]?.ToString()
            ?? context.Items["user"]?.ToString()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "127.0.0.1";
    }
}
