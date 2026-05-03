using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Monitoring.Presentation.Hubs;

/// <summary>
/// SignalR hub para broadcast de dados em tempo real para o dashboard e app mobile.
/// </summary>
public sealed class MonitoringHub : Hub { }