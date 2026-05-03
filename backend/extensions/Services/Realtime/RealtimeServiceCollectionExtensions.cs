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

        services
            .AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.AddFilter<HubExceptionFilter>();
                options.AddFilter<HubLoggingFilter>();
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.SnakeCaseLower;
                options.PayloadSerializerOptions.DictionaryKeyPolicy =
                    JsonNamingPolicy.SnakeCaseLower;
            });

        services.AddSingleton<IRealtimeService, RealtimeService>();

        return services;
    }
}
