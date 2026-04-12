namespace backend.Extensions.Auth.Abstractions;

public interface IUserRoleProvider
{
    Task<string?> GetRoleByEmailAsync(string email, CancellationToken cancellationToken = default);
}
