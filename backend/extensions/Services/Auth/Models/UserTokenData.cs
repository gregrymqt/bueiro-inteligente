namespace backend.extensions.Services.Auth.Models;

public sealed class UserTokenData
{
    public UserTokenData(string email, IReadOnlyList<string> roles, string jti, Guid? userId = null)
    {
        Email = email;
        Roles = NormalizeRoles(roles);
        Jti = jti;
        UserId = userId;
    }

    public string Email { get; }

    public Guid? UserId { get; }

    public IReadOnlyList<string> Roles { get; set; }

    public string Role => Roles.FirstOrDefault() ?? "User";

    public string Jti { get; }

    private static IReadOnlyList<string> NormalizeRoles(IEnumerable<string> roles)
    {
        List<string> normalizedRoles = [];
        normalizedRoles.AddRange(from role in roles where !string.IsNullOrWhiteSpace(role) select role.Trim());

        return normalizedRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}