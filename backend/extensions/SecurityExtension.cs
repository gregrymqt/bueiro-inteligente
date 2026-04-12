using System.Collections.Concurrent;
using System.Security.Claims;
using backend.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace backend.Extensions;

public sealed class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message, TimeSpan? retryAfter = null)
        : base(message)
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan? RetryAfter { get; }
}

public interface IRateLimiter
{
    Task EnforceAsync(HttpContext context, CancellationToken cancellationToken = default);
}

public interface IRateLimitStore
{
    Task<int?> GetCountAsync(string key, CancellationToken cancellationToken = default);

    Task IncrementAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);
}

public sealed class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new(
        StringComparer.Ordinal
    );

    public Task<int?> GetCountAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(key, out RateLimitEntry? entry))
        {
            return Task.FromResult<int?>(null);
        }

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return Task.FromResult<int?>(null);
        }

        return Task.FromResult<int?>(entry.Count);
    }

    public Task IncrementAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry(1, now.Add(ttl)),
            (_, existing) =>
            {
                if (existing.ExpiresAt <= now)
                {
                    return new RateLimitEntry(1, now.Add(ttl));
                }

                return existing with
                {
                    Count = existing.Count + 1,
                };
            }
        );

        return Task.CompletedTask;
    }

    private sealed record RateLimitEntry(int Count, DateTimeOffset ExpiresAt);
}

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
            string identifier = ResolveIdentifier(context);
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

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteSecurity(
        this IServiceCollection services,
        int times = 5,
        int seconds = 10
    )
    {
        services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
        services.AddSingleton(sp => new RateLimiter(
            sp.GetRequiredService<IRateLimitStore>(),
            sp.GetRequiredService<ILogger<RateLimiter>>(),
            times,
            seconds
        ));
        services.AddSingleton<IRateLimiter>(sp => sp.GetRequiredService<RateLimiter>());
        services.AddSingleton(sp => new WebSocketRateLimiter(
            sp.GetRequiredService<IRateLimitStore>(),
            sp.GetRequiredService<ILogger<WebSocketRateLimiter>>(),
            times,
            seconds
        ));

        return services;
    }

    public static void InitializeBueiroInteligenteSecurity(this IServiceProvider serviceProvider)
    {
        _ = serviceProvider.GetRequiredService<RateLimiter>();
        _ = serviceProvider.GetRequiredService<WebSocketRateLimiter>();
    }
}
