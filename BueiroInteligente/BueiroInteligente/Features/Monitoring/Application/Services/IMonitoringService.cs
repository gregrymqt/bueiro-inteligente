using BueiroInteligente.Features.Monitoring.Application.DTOs;

namespace BueiroInteligente.Features.Monitoring.Application.Services;

/// <summary>
/// Application contract for monitoring use cases.
/// </summary>
public interface IMonitoringService
{
    /// <summary>Processes a raw sensor reading, persists it, and broadcasts alerts when needed.</summary>
    Task<DrainStatusDTO> ProcessSensorDataAsync(
        SensorPayloadDTO payload,
        CancellationToken cancellationToken = default
    );

    /// <summary>Gets the current status for a drain using cache-aside semantics.</summary>
    Task<DrainStatusDTO> GetDrainStatusAsync(
        string drainIdentifier,
        CancellationToken cancellationToken = default
    );
}