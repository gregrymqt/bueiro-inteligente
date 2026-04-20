using backend.Features.Monitoring.Application.Services;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Monitoring.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions;

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
