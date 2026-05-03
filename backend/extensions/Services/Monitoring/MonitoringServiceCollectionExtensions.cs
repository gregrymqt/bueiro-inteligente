using backend.Features.Monitoring.Application.Interfaces;
using backend.Features.Monitoring.Application.Services;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Monitoring.Infrastructure.Repositories;

namespace backend.extensions.Services.Monitoring;

/// <summary>
/// Registers the Monitoring vertical slice services.
/// </summary>
public static class MonitoringServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteMonitoring(
        this IServiceCollection services
    )
    {
        services.AddScoped<IMonitoringRepository, MonitoringRepository>();
        services.AddScoped<IMonitoringService, MonitoringService>();

        return services;
    }
}
