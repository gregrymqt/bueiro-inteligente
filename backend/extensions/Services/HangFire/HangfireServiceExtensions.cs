using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Scheduler.Application.Services;
using Hangfire;
using Hangfire.Redis.StackExchange;

namespace backend.extensions.Services.HangFire;

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

        // 🚀 Registrando a nossa Queue globalmente para ser usada por qualquer Feature
        services.AddScoped<IQueueService, BackgroundJobQueueService>();

        return services;
    }
}
