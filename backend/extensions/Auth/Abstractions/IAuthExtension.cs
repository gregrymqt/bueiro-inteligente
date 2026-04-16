using backend.Extensions.Auth.Models;

namespace backend.Extensions.Auth.Abstractions;

public interface IAuthExtension
{
    Task<bool> VerifyPasswordAsync(string plainPassword, string hashedPassword);

    Task<string> GetPasswordHashAsync(string password);

    Task<UserTokenData> GetCurrentUserAsync(string token, CancellationToken cancellationToken = default);

    string CreateAccessToken(TokenPayload payload);

    Task AddToBlacklistAsync(string jti);

    Task<bool> IsBlacklistedAsync(string jti);

    string VerifyHardwareToken(string? authorizationToken = null, string? queryToken = null);
}
