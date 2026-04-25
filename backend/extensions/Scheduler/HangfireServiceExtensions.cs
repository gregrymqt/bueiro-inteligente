using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.Scheduler;

public static class HangfireServiceExtensions
{
    public static IServiceCollection AddBueiroInteligenteHangfire(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var redisConnectionString =
            configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' não definida.");

        services.AddHangfire(options =>
            options.UseRedisStorage(
                redisConnectionString,
                new RedisStorageOptions { Prefix = "{bueiro-inteligente}:hangfire:" }
            )
        );

        services.AddHangfireServer();

        return services;
    }
}
