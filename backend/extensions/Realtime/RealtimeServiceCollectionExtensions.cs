using backend.Extensions.Realtime.Abstractions;
using backend.Extensions.Realtime.Infrastructure;
using backend.Features.Realtime.Filters;
using backend.Features.Realtime.Presentation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace backend.Extensions.Realtime;

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
