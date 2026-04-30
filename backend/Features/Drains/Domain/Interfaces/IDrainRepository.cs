using DrainEntity = global::backend.Features.Drain.Domain.Drain;

namespace backend.Features.Drains.Domain.Interfaces;

public interface IDrainRepository
{
    Task<DrainEntity?> GetByIdAsync(Guid drainId, CancellationToken ct = default);
    Task<DrainEntity?> GetByHardwareIdAsync(string hardwareId, CancellationToken ct = default);
    Task<IReadOnlyList<DrainEntity>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken ct = default);
    Task<DrainEntity> CreateAsync(DrainEntity drain, CancellationToken ct = default);
    Task<DrainEntity> UpdateAsync(DrainEntity drain, CancellationToken ct = default);
    Task DeleteAsync(DrainEntity drain, CancellationToken ct = default);
    Task<IReadOnlyList<backend.Features.Drains.Application.DTOs.DrainLookupDTO>> GetAvailableDrainsAsync(CancellationToken ct = default);
}