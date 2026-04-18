using backend.Features.Monitoring.Application.DTOs;

namespace backend.Features.Monitoring.Application.Services;

public interface IMonitoringService
{
    Task<DrainStatusDTO> ProcessSensorDataAsync(
        SensorPayloadDTO payload,
        CancellationToken ct = default
    );
    Task<DrainStatusDTO> GetDrainStatusAsync(
        string drainIdentifier,
        CancellationToken ct = default
    );
}
