using backend.Features.Monitoring.Application.DTOs;

namespace backend.Features.Monitoring.Application.Interfaces;

public interface IMonitoringIngestionService
{
    Task<bool> SaveSensorDataAsync(DrainStatusDTO data, CancellationToken ct = default);
}