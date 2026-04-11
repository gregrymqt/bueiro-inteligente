using BueiroInteligente.Features.Drains.Application.DTOs;

namespace BueiroInteligente.Features.Drains.Domain.Interfaces;

public interface IDrainService
{
    Task<IReadOnlyList<DrainResponse>> GetAllDrainsAsync(
        int skip = 0,
        int limit = 100,
        CancellationToken cancellationToken = default
    );

    Task<DrainResponse> GetDrainByIdAsync(
        Guid drainId,
        CancellationToken cancellationToken = default
    );

    Task<DrainResponse> CreateDrainAsync(
        DrainCreateRequest request,
        CancellationToken cancellationToken = default
    );

    Task<DrainResponse> UpdateDrainAsync(
        Guid drainId,
        DrainUpdateRequest request,
        CancellationToken cancellationToken = default
    );

    Task DeleteDrainAsync(Guid drainId, CancellationToken cancellationToken = default);
}
