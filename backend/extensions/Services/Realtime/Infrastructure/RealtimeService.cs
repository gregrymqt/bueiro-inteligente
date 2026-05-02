using backend.Core;
using backend.Extensions.Realtime.Abstractions;
using backend.Features.Realtime.Presentation;
using Microsoft.AspNetCore.SignalR;

namespace backend.Extensions.Realtime.Infrastructure;

public sealed class RealtimeService(IHubContext<MonitoringHub> hubContext) : IRealtimeService
{
    private const string EventName = "BUEIRO_STATUS_MUDOU";

    public async Task BroadcastMonitoringData(object data)
    {
        // Validação rápida de nulidade
        _ = data ?? throw LogicException.NullValue(nameof(data));

        try
        {
            // O parâmetro 'hubContext' já está disponível no escopo da classe pelo Primary Constructor
            await hubContext.Clients.All.SendAsync(EventName, data);
        }
        catch (Exception ex)
        {
            throw new ConnectionException(
                "SignalR",
                "Falha ao transmitir dados de monitoramento em tempo real.",
                ex
            );
        }
    }
}
