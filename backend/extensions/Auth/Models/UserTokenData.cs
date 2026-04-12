namespace backend.Extensions.Auth.Models;

public sealed class UserTokenData
{
    public UserTokenData(string email, string role, string jti)
    {
        Email = email;
        Role = role;
        Jti = jti;
    }

    public string Email { get; }

    public string Role { get; set; }

    public string Jti { get; }
}
