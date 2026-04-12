using backend.Core;
using backend.Features.Auth.Domain;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Auth.Infrastructure.Repositories;

public sealed class AuthRepository(AppDbContext dbContext, ILogger<AuthRepository> logger)
    : IAuthRepository
{
    private readonly AppDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<AuthRepository> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .Users.AsNoTracking()
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving user by email {Email}", email);
            throw new ConnectionException(
                "AuthRepository.GetUserByEmailAsync",
                $"Failed to query user by email '{email}'.",
                exception
            );
        }
    }

    public async Task<Role?> GetRoleByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .Roles.AsNoTracking()
                .FirstOrDefaultAsync(role => role.Name == roleName, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving role by name {RoleName}", roleName);
            throw new ConnectionException(
                "AuthRepository.GetRoleByNameAsync",
                $"Failed to query role '{roleName}'.",
                exception
            );
        }
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            await _dbContext.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error adding user {Email}", user.Email);
            throw new ConnectionException(
                "AuthRepository.AddUserAsync",
                $"Failed to add user '{user.Email}'.",
                exception
            );
        }
    }
}
