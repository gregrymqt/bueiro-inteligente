using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Configuration;
using backend.Features.Monitoring.Domain.Entities;

namespace backend.Features.Monitoring.Domain.Interfaces;

public interface IMonitoringRepository
{
    Task InsertAsync(DrainStatus entity, CancellationToken ct = default);
    Task<DrainStatusDTO?> GetLatestStatusAsync(string drainId, CancellationToken ct = default);
    Task<IReadOnlyList<DrainStatusDTO>> GetUnsyncedDataAsync(
        int limit = 100,
        CancellationToken ct = default
    );
    Task MarkAsSyncedAsync(IReadOnlyCollection<string> drainIds, CancellationToken ct = default);
    Task<BueiroConfiguration> GetConfigByIdAsync(string id, CancellationToken ct = default);
}
