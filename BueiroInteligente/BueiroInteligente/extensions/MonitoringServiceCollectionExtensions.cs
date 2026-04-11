using BueiroInteligente.Features.Monitoring.Application.Services;
using BueiroInteligente.Features.Monitoring.Domain.Interfaces;
using BueiroInteligente.Features.Monitoring.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BueiroInteligente.Extensions;

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