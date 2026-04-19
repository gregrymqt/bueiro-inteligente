using backend.Extensions.Realtime.Abstractions;
using backend.Extensions.Realtime.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.Realtime;

public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRealtime(this IServiceCollection services)
    {
        services.AddSingleton<IRealtimeService, RealtimeService>();

        return services;
    }
}
