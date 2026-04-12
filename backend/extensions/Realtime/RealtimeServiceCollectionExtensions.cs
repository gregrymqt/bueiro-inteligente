using backend.Extensions.Realtime.Abstractions;
using backend.Extensions.Realtime.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.Realtime;

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
