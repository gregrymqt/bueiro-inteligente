using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Configuration;

namespace backend.Features.Monitoring.Domain.Interfaces;

public interface IMonitoringRepository
{
    Task SaveSensorDataAsync(DrainStatusDTO data, CancellationToken ct = default);
    Task<DrainStatusDTO?> GetLatestStatusAsync(string drainId, CancellationToken ct = default);
    Task<IReadOnlyList<DrainStatusDTO>> GetUnsyncedDataAsync(
        int limit = 100,
        CancellationToken ct = default
    );
    Task MarkAsSyncedAsync(IReadOnlyCollection<string> drainIds, CancellationToken ct = default);
    Task<BueiroConfiguration> GetConfigByIdAsync(string id, CancellationToken ct = default);
}
