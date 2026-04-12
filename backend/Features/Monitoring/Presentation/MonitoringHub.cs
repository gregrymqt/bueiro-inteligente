using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Monitoring.Presentation;

/// <summary>
/// SignalR hub used by the dashboard and the Kotlin app to receive monitoring data.
/// </summary>
public sealed class MonitoringHub : Hub { }