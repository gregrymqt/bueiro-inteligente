using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace backend.Infrastructure;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRedis(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connectionString =
                configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Connection string 'Redis' não definida.");

            return ConnectionMultiplexer.Connect(connectionString);
        });
    }

    public static async Task InitializeBueiroInteligenteRedisAsync(
        this IServiceProvider sp,
        CancellationToken ct = default
    )
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("RedisBootstrap");
        try
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            await redis.GetDatabase().PingAsync().WaitAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis indisponível. O sistema utilizará fallback de cache.");
        }
    }

}
