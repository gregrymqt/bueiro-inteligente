using StackExchange.Redis;

namespace backend.Infrastructure.Extensions;

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

            var options = ConfigurationOptions.Parse(connectionString, true);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = Math.Max(options.ConnectRetry, 5);
            options.ConnectTimeout = Math.Max(options.ConnectTimeout, 10_000);
            options.AsyncTimeout = Math.Max(options.AsyncTimeout, 10_000);
            options.KeepAlive = options.KeepAlive > 0 ? options.KeepAlive : 60;
            options.ReconnectRetryPolicy = new ExponentialRetry(1_000);

            return ConnectionMultiplexer.Connect(options);
        });
    }

    public static async Task InitializeBueiroInteligenteRedisAsync(
        this IServiceProvider sp,
        CancellationToken ct = default
    )
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("RedisBootstrap");

        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                await redis.GetDatabase().PingAsync().WaitAsync(ct).ConfigureAwait(false);

                logger.LogInformation("Redis inicializado com sucesso.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Redis indisponível na tentativa {Attempt}/{MaxAttempts}. Retentando em {Delay}s...",
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
                    "Falha ao inicializar o Redis após {MaxAttempts} tentativas. A aplicação continuará sem warm-up do cache/rate limit.",
                    maxAttempts
                );

                return;
            }
        }
    }

}
