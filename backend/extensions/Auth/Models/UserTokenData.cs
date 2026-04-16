namespace backend.Extensions.Auth.Models;

public sealed class UserTokenData
{
    public UserTokenData(string email, IReadOnlyList<string> roles, string jti)
    {
        Email = email;
        Roles = NormalizeRoles(roles);
        Jti = jti;
    }

    public string Email { get; }

    public IReadOnlyList<string> Roles { get; set; }

    public string Role => Roles.FirstOrDefault() ?? "User";

    public string Jti { get; }

    private static IReadOnlyList<string> NormalizeRoles(IEnumerable<string> roles)
    {
        List<string> normalizedRoles = new();

        foreach (string role in roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                normalizedRoles.Add(role.Trim());
            }
        }

        return normalizedRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
