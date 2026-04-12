using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions;

/// <summary>
/// Registers the realtime broadcast service.
/// </summary>
public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRealtime(this IServiceCollection services)
    {
        services.AddSingleton<IRealtimeService, RealtimeService>();

        return services;
    }
}