using backend.Extensions.Security.Abstractions;
using backend.Extensions.Security.Infrastructure;
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
