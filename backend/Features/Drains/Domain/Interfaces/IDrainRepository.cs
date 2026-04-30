
namespace backend.Features.Drains.Domain.Interfaces;

public interface IDrainRepository
{
    Task<Drain?> GetByIdAsync(Guid drainId, CancellationToken ct = default);
    Task<Drain?> GetByHardwareIdAsync(string hardwareId, CancellationToken ct = default);
    Task<IReadOnlyList<Drain>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken ct = default);
    Task<Drain> CreateAsync(Drain drain, CancellationToken ct = default);
    Task<Drain> UpdateAsync(Drain drain, CancellationToken ct = default);
    Task DeleteAsync(Drain drain, CancellationToken ct = default);
}