namespace backend.Extensions.Realtime.Abstractions;

public interface IRealtimeService
{
    Task BroadcastMonitoringData(object data);
}