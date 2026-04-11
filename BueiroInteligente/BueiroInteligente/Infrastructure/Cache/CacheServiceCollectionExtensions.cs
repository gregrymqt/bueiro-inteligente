using Microsoft.Extensions.DependencyInjection;

namespace BueiroInteligente.Infrastructure.Cache;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteCache(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}