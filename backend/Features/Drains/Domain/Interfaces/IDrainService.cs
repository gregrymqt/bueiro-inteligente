using backend.Features.Drains.Application.DTOs;

namespace backend.Features.Drains.Domain.Interfaces;

public interface IDrainService
{
    Task<IReadOnlyList<DrainResponse>> GetAllDrainsAsync(int skip = 0, int limit = 100, CancellationToken ct = default);
    Task<DrainResponse> GetDrainByIdAsync(Guid drainId, CancellationToken ct = default);
    Task<DrainResponse> CreateDrainAsync(DrainCreateRequest request, Guid userId, CancellationToken ct = default);
    Task<DrainResponse> UpdateDrainAsync(Guid drainId, DrainUpdateRequest request, CancellationToken ct = default);
    Task DeleteDrainAsync(Guid drainId, CancellationToken ct = default);
}