using backend.Core;
using backend.Features.Auth.Domain;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Auth.Infrastructure.Repositories;

// C# 12: O construtor primário já define o escopo dos campos privados necessários.
public sealed class AuthRepository(AppDbContext dbContext, ILogger<AuthRepository> logger)
    : IAuthRepository
{
    public async Task<User?> FindByGoogleIdAsync(string googleId, CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Users
                .AsNoTracking()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by Google id {GoogleId}", googleId);
            throw new ConnectionException("AuthRepository.FindByGoogleIdAsync", $"Failed to query user by Google id '{googleId}'.", ex);
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Users
                .AsNoTracking()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by email {Email}", email);
            throw new ConnectionException("AuthRepository.GetUserByEmailAsync", $"Failed to query user by email '{email}'.", ex);
        }
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving role by name {RoleName}", roleName);
            throw new ConnectionException("AuthRepository.GetRoleByNameAsync", $"Failed to query role '{roleName}'.", ex);
        }
    }

    public async Task AddUserAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            // C# 12: Simplificação do anexo de entidades relacionadas (Roles)
            foreach (var role in user.Roles.Where(r => r is not null).DistinctBy(r => r.Id))
            {
                if (dbContext.Entry(role).State == EntityState.Detached)
                    dbContext.Attach(role);
            }

            await dbContext.Users.AddAsync(user, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding user {Email}", user.Email);
            throw new ConnectionException("AuthRepository.AddUserAsync", $"Failed to add user '{user.Email}'.", ex);
        }
    }
}