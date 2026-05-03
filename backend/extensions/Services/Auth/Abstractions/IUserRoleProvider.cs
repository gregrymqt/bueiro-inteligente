namespace backend.extensions.Services.Auth.Abstractions;

public interface IUserRoleProvider
{
    Task<IReadOnlyList<string>?> GetRolesByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    );
}
