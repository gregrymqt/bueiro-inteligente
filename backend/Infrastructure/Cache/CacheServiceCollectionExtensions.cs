using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.Cache;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteCache(this IServiceCollection services)
    {
        // O RedisCacheService deve ser Singleton para manter a conexão ativa
        return services.AddSingleton<ICacheService, RedisCacheService>();
    }
}
