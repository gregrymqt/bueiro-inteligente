using backend.Features.Auth.Domain.Entities;

namespace backend.Features.Auth.Domain.Interfaces;

public interface IAuthRepository
{
    Task<User?> FindByGoogleIdAsync(string googleId, CancellationToken ct = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);
    Task AddUserAsync(User user, CancellationToken ct = default);
}