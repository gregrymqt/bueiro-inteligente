using backend.Core;
using backend.extensions.Services.Realtime.Abstractions;
using backend.Features.Realtime.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace backend.extensions.Services.Realtime.Infrastructure;

public sealed class RealtimeService(IHubContext<ApplicationHub> hubContext, ILogger<RealtimeService> logger) : IRealtimeService
{
    public async Task PublishAsync(string eventName, object data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        try
        {
            // Envia para todos: Dashboard global, mapas, etc.
            await hubContext.Clients.All.SendAsync(eventName, data).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha no Broadcast Global: {Event}", eventName);
        }
    }

    public async Task PublishToUserAsync(string userId, string eventName, object data)
    {
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            // Envia apenas para o dono da assinatura ou do pagamento
            await hubContext.Clients.User(userId).SendAsync(eventName, data).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar evento {Event} para o usuário {User}", eventName, userId);
        }
    }
}