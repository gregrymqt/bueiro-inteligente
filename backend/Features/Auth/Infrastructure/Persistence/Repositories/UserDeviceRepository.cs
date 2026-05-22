using backend.Features.Users.Domain.Entities;
using backend.Features.Users.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Users.Infrastructure.Persistence.Repositories;

public sealed class UserDeviceRepository(AppDbContext dbContext) : IUserDeviceRepository
{
    public async Task<UserDevice?> GetByTokenAsync(string fcmToken, CancellationToken ct = default)
    {
        return await dbContext.UserDevices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.FcmToken == fcmToken, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetTokensByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.UserDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Select(d => d.FcmToken)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(UserDevice userDevice, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userDevice);
        await dbContext.UserDevices.AddAsync(userDevice, ct).ConfigureAwait(false);
    }
}