using DrainEntity = global::BueiroInteligente.Features.Drain.Domain.Drain;

namespace BueiroInteligente.Features.Drains.Domain.Interfaces;

public interface IDrainRepository
{
    Task<DrainEntity?> GetByIdAsync(Guid drainId, CancellationToken cancellationToken = default);

    Task<DrainEntity?> GetByHardwareIdAsync(
        string hardwareId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<DrainEntity>> GetAllAsync(
        int skip = 0,
        int limit = 100,
        CancellationToken cancellationToken = default
    );

    Task<DrainEntity> CreateAsync(DrainEntity drain, CancellationToken cancellationToken = default);

    Task<DrainEntity> UpdateAsync(DrainEntity drain, CancellationToken cancellationToken = default);

    Task DeleteAsync(DrainEntity drain, CancellationToken cancellationToken = default);
}
