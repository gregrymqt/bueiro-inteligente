using System.Text.Json;
using backend.extensions.Services.Realtime.Abstractions;
using backend.extensions.Services.Realtime.Infrastructure;
using backend.Features.Realtime.Filters;
using Microsoft.AspNetCore.SignalR;

namespace backend.extensions.Services.Realtime;

public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRealtime(this IServiceCollection services)
    {
        services.AddSingleton<HubExceptionFilter>();
        services.AddSingleton<HubLoggingFilter>();

        services.AddSignalR(options =>
            {
                options.AddFilter<HubExceptionFilter>();
                options.AddFilter<HubLoggingFilter>();
            })
            .AddJsonProtocol(options =>
            {
                // Mantém o padrão snake_case que seu frontend já espera[cite: 28]
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            });

        services.AddSingleton<IRealtimeService, RealtimeService>();
        return services;
    }
}
