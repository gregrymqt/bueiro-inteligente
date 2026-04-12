using System.Security.Claims;
using backend.Core;
using backend.Extensions.Security.Abstractions;
using backend.Extensions.Security.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace backend.Extensions.Security.Infrastructure;

public sealed class RateLimiter : IRateLimiter
{
    private readonly IRateLimitStore _rateLimitStore;
    private readonly ILogger<RateLimiter> _logger;

    public RateLimiter(
        IRateLimitStore rateLimitStore,
        ILogger<RateLimiter> logger,
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
            string identifier = ResolveIdentifier(context);
            string routePath = context.Request.Path.Value ?? "/";
            string key = $"rate_limit:{identifier}:{routePath}";

            int? currentCount = await _rateLimitStore
                .GetCountAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (currentCount.HasValue && currentCount.Value >= Times)
            {
                throw new RateLimitExceededException(
                    "Too Many Requests. Limite de requisições excedido.",
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
            _logger.LogError(exception, "Erro no validador do Redis (RateLimiter).");
        }
    }

    private static string ResolveIdentifier(HttpContext context)
    {
        string forwarded = context.Request.Headers["X-Forwarded-For"].ToString();

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            string? claimUserId =
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue(ClaimTypes.Name);

            if (!string.IsNullOrWhiteSpace(claimUserId))
            {
                return claimUserId;
            }
        }

        if (context.Items.TryGetValue("user_id", out object? userId) && userId is not null)
        {
            return userId.ToString() ?? "127.0.0.1";
        }

        if (context.Items.TryGetValue("user", out object? user) && user is not null)
        {
            return user.ToString() ?? "127.0.0.1";
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}