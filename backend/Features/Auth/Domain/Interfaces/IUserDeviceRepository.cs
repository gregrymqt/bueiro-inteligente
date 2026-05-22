using backend.Features.Users.Domain.Entities;

namespace backend.Features.Users.Domain.Interfaces;

public interface IUserDeviceRepository
{
    Task<UserDevice?> GetByTokenAsync(string fcmToken, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetTokensByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserDevice userDevice, CancellationToken ct = default);
}