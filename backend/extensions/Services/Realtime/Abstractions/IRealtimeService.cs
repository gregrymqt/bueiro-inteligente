namespace backend.extensions.Services.Realtime.Abstractions;

public interface IRealtimeService
{
    Task BroadcastMonitoringData(object data);
}