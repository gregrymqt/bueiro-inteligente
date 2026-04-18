using backend.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace backend.Infrastructure;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRedis(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);
        return services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(BuildOptions(sp.GetRequiredService<AppSettings>()))
        );
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

    private static ConfigurationOptions BuildOptions(AppSettings settings)
    {
        if (settings.RedisLocal)
            return ConfigurationOptions.Parse("redis:6379,abortConnect=false");

        if (string.IsNullOrWhiteSpace(settings.RedisUrl))
            throw new InvalidOperationException("REDIS_URL não definida.");

        if (Uri.TryCreate(settings.RedisUrl, UriKind.Absolute, out var uri))
        {
            var options = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                Ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase),
            };

            options.EndPoints.Add(uri.Host, uri.IsDefaultPort ? 6379 : uri.Port);

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                var parts = uri.UserInfo.Split(':', 2);
                options.User = Uri.UnescapeDataString(parts[0]);
                if (parts.Length > 1)
                    options.Password = Uri.UnescapeDataString(parts[1]);
            }

            if (int.TryParse(uri.AbsolutePath.Trim('/'), out int dbIndex))
                options.DefaultDatabase = dbIndex;
            return options;
        }

        return ConfigurationOptions.Parse(settings.RedisUrl);
    }
}
