using backend.Core;
using backend.Features.Realtime.Presentation;
using Microsoft.AspNetCore.SignalR;

namespace backend.Extensions;

/// <summary>
/// Default SignalR implementation used to push monitoring updates to clients.
/// </summary>
public sealed class RealtimeService(IHubContext<MonitoringHub> hubContext) : IRealtimeService
{
    private const string MonitoringStatusChangedEventName = "BUEIRO_STATUS_MUDOU";

    private readonly IHubContext<MonitoringHub> _hubContext =
        hubContext ?? throw new ArgumentNullException(nameof(hubContext));

    public async Task BroadcastMonitoringData(object data)
    {
        if (data is null)
        {
            throw LogicException.NullValue(nameof(data));
        }

        try
        {
            await _hubContext.Clients.All.SendAsync(MonitoringStatusChangedEventName, data);
        }
        catch (Exception exception)
        {
            throw new ConnectionException(
                "SignalR",
                "Falha ao transmitir dados de monitoring para os clientes.",
                exception
            );
        }
    }
}