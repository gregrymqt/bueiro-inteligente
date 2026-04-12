using backend.Features.Monitoring.Application.DTOs;

namespace backend.Features.Monitoring.Domain.Interfaces;

/// <summary>
/// Persistence contract for monitoring sensor data and historical drain status.
/// </summary>
public interface IMonitoringRepository
{
    /// <summary>Saves the current sensor reading in Redis and appends a historical record.</summary>
    Task SaveSensorDataAsync(DrainStatusDTO data, CancellationToken cancellationToken = default);

    /// <summary>Gets the latest known status for a drain.</summary>
    Task<DrainStatusDTO?> GetLatestStatusAsync(
        string drainIdentifier,
        CancellationToken cancellationToken = default
    );

    /// <summary>Gets pending drain records that have not yet been synchronized with Rows.</summary>
    Task<IReadOnlyList<DrainStatusDTO>> GetUnsyncedDataAsync(
        int limit = 100,
        CancellationToken cancellationToken = default
    );

    /// <summary>Marks the provided drain identifiers as synchronized with Rows.</summary>
    Task MarkAsSyncedAsync(
        IReadOnlyCollection<string> drainIdentifiers,
        CancellationToken cancellationToken = default
    );
}