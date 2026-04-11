using BueiroInteligente.Features.Auth.Domain;

namespace BueiroInteligente.Features.Auth.Infrastructure.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
}