namespace BueiroInteligente.Extensions;

/// <summary>
/// Contract used to broadcast monitoring data to connected SignalR clients.
/// </summary>
public interface IRealtimeService
{
    /// <summary>Broadcasts monitoring data to every connected client.</summary>
    Task BroadcastMonitoringData(object data);
}