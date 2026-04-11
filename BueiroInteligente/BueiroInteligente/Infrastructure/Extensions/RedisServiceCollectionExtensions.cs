using BueiroInteligente.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BueiroInteligente.Infrastructure;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRedis(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            AppSettings settings = sp.GetRequiredService<AppSettings>();
            ConfigurationOptions configurationOptions = BuildConfigurationOptions(settings);

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        return services;
    }

    public static async Task InitializeBueiroInteligenteRedisAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        ILogger logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RedisBootstrap");

        try
        {
            IConnectionMultiplexer connectionMultiplexer =
                serviceProvider.GetRequiredService<IConnectionMultiplexer>();
            IDatabase database = connectionMultiplexer.GetDatabase();

            await database.PingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis indisponível durante a inicialização. O sistema continuará com fallback de cache."
            );
        }
    }

    private static ConfigurationOptions BuildConfigurationOptions(AppSettings settings)
    {
        if (settings.RedisLocal)
        {
            return ConfigurationOptions.Parse("redis:6379,abortConnect=false");
        }

        if (string.IsNullOrWhiteSpace(settings.RedisUrl))
        {
            throw new InvalidOperationException("REDIS_URL não está definida.");
        }

        string normalizedUrl = settings.RedisUrl.Trim();

        if (
            normalizedUrl.Contains("://", StringComparison.Ordinal)
            && Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri? uri)
            && uri.Scheme.StartsWith("redis", StringComparison.OrdinalIgnoreCase)
        )
        {
            var configurationOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
                KeepAlive = 180,
                Ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase),
            };

            configurationOptions.EndPoints.Add(uri.Host, uri.IsDefaultPort ? 6379 : uri.Port);

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                string[] credentials = uri.UserInfo.Split(':', 2);

                if (credentials.Length > 0)
                {
                    configurationOptions.User = Uri.UnescapeDataString(credentials[0]);
                }

                if (credentials.Length > 1)
                {
                    configurationOptions.Password = Uri.UnescapeDataString(credentials[1]);
                }
            }

            if (int.TryParse(uri.AbsolutePath.Trim('/'), out int databaseIndex))
            {
                configurationOptions.DefaultDatabase = databaseIndex;
            }

            return configurationOptions;
        }

        ConfigurationOptions parsedOptions = ConfigurationOptions.Parse(normalizedUrl);
        parsedOptions.AbortOnConnectFail = false;
        parsedOptions.ConnectRetry = 3;
        parsedOptions.ConnectTimeout = 5000;
        parsedOptions.SyncTimeout = 5000;
        parsedOptions.KeepAlive = 180;

        return parsedOptions;
    }
}
