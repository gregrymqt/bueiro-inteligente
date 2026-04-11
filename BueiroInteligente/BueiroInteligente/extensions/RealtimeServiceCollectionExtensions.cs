using Microsoft.Extensions.DependencyInjection;

namespace BueiroInteligente.Extensions;

/// <summary>
/// Registers SignalR and the realtime broadcast service.
/// </summary>
public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRealtime(this IServiceCollection services)
    {
        services.AddSignalR(options => options.EnableDetailedErrors = true);
        services.AddSingleton<IRealtimeService, RealtimeService>();

        return services;
    }
}