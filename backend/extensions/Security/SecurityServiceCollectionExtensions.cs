using System.Net.Sockets;
using backend.Extensions.Security.Abstractions;
using backend.Extensions.Security.Infrastructure;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace backend.Extensions.Security;

/// <summary>
/// Registers rate-limit and websocket security services.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteSecurity(
        this IServiceCollection services,
        int times = 5,
        int seconds = 10
    )
    {
        services.AddScoped<RateLimitFilter>();
        services.AddScoped<IRateLimitStore, RedisRateLimitStore>();
        services.AddScoped(sp => new RateLimiter(
            sp.GetRequiredService<IRateLimitStore>(),
            sp.GetRequiredService<ILogger<RateLimiter>>(),
            times,
            seconds
        ));
        services.AddScoped<IRateLimiter>(sp => sp.GetRequiredService<RateLimiter>());
        services.AddScoped(sp => new WebSocketRateLimiter(
            sp.GetRequiredService<IRateLimitStore>(),
            sp.GetRequiredService<ILogger<WebSocketRateLimiter>>(),
            times,
            seconds
        ));

        return services;
    }

    public static async Task InitializeBueiroInteligenteSecurityAsync(
        this IServiceProvider serviceProvider,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SecurityBootstrap");

        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _ = serviceProvider.GetRequiredService<RateLimiter>();
                _ = serviceProvider.GetRequiredService<WebSocketRateLimiter>();

                logger.LogInformation("Rate limiter inicializado com sucesso.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientRedisBootstrapError(ex))
            {
                logger.LogWarning(
                    ex,
                    "Rate limiter ainda não está pronto na tentativa {Attempt}/{MaxAttempts}. Retentando em {Delay}s...",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds
                );

                await Task.Delay(delay, ct).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
            }
            catch (Exception ex)
            {
                logger.LogCritical(
                    ex,
                    "Falha ao inicializar o rate limiter após {MaxAttempts} tentativas. A aplicação continuará sem warm-up de segurança.",
                    maxAttempts
                );

                return;
            }
        }
    }

    private static bool IsTransientRedisBootstrapError(Exception exception)
    {
        return exception is RedisConnectionException
            or SocketException
            or TimeoutException
            || exception.InnerException is RedisConnectionException
            || exception.InnerException is SocketException
            || exception.InnerException is TimeoutException;
    }
}
