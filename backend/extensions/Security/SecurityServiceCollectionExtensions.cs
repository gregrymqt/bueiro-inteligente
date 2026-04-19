using backend.Extensions.Security.Abstractions;
using backend.Extensions.Security.Infrastructure;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    public static void InitializeBueiroInteligenteSecurity(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        _ = scope.ServiceProvider.GetRequiredService<RateLimiter>();
        _ = scope.ServiceProvider.GetRequiredService<WebSocketRateLimiter>();
    }
}
