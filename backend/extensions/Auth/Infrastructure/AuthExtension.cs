using System.Text.Json;
using backend.Core;
using backend.Core.Settings;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Models;
using backend.Extensions.Auth.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace backend.Extensions.Auth;

public sealed class AuthExtension(
    IOptions<JwtSettings> jwtSettings,
    IOptions<IotSettings> iotSettings,
    ITokenBlacklistStore blacklistStore,
    IPasswordHasher<object> passwordHasher,
    ILogger<AuthExtension> logger
) : IAuthExtension
{
    private readonly TimeSpan _blacklistTtl = TimeSpan.FromMinutes(
        jwtSettings.Value.AccessTokenExpireMinutes + 1
    );

    public Task OpenAsync()
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Value.SecretKey))
            logger.LogWarning("ALERTA: SECRET_KEY insegura.");

        if (string.IsNullOrWhiteSpace(iotSettings.Value.HardwareToken))
            logger.LogWarning("ALERTA: HARDWARE_TOKEN ausente.");

        return Task.CompletedTask;
    }

    public string CreateAccessToken(TokenPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Email))
            throw LogicException.InvalidValue(nameof(payload.Email), payload.Email);

        var now = DateTimeOffset.UtcNow;
        var claims = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["sub"] = payload.Email,
            ["role"] = payload.Role ?? "User",
            ["exp"] = now.AddMinutes(jwtSettings.Value.AccessTokenExpireMinutes)
                .ToUnixTimeSeconds(),
            ["iat"] = now.ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString(),
        };

        if (payload.AdditionalClaims is not null)
        {
            foreach (var claim in payload.AdditionalClaims)
                claims.TryAdd(claim.Key, claim.Value);
        }
        return JwtTokenHelper.Encode(
            claims,
            jwtSettings.Value.SecretKey!,
            jwtSettings.Value.Algorithm
        );
    }

    public Task<bool> VerifyPasswordAsync(string plain, string hashed) =>
        Task.FromResult(
            passwordHasher.VerifyHashedPassword(new object(), hashed, plain)
                == PasswordVerificationResult.Success
        );

    public Task<string> GetPasswordHashAsync(string password) =>
        Task.FromResult(passwordHasher.HashPassword(new object(), password));

    public async Task<UserTokenData> GetCurrentUserAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedToken = NormalizeBearerToken(token) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedToken))
        {
            throw new UnauthorizedAccessException("Token JWT vazio.");
        }

        Dictionary<string, object?> claims = JwtTokenHelper.Decode(
            normalizedToken,
            jwtSettings.Value.SecretKey!,
            jwtSettings.Value.Algorithm
        );

        string email = GetRequiredClaim(claims, "sub", "email");
        string jti = GetRequiredClaim(claims, "jti");
        Guid? userId = TryGetOptionalGuidClaim(claims, "user_id");

        if (await IsBlacklistedAsync(jti).ConfigureAwait(false))
        {
            throw new UnauthorizedAccessException("Token revogado.");
        }

        IReadOnlyList<string> roles = TryGetClaimAsStringList(claims, "roles");

        if (roles.Count == 0)
        {
            roles = TryGetClaimAsStringList(claims, "role");
        }

        if (roles.Count == 0)
        {
            roles = ["User"];
        }

        return new UserTokenData(email, roles, jti, userId);
    }

    public string VerifyHardwareToken(string? auth = null, string? query = null)
    {
        var token = NormalizeBearerToken(auth) ?? NormalizeBearerToken(query);

        if (string.IsNullOrEmpty(token) || token != iotSettings.Value.HardwareToken)
            throw new UnauthorizedAccessException("Hardware não autorizado.");

        return token;
    }

    public static string? NormalizeBearerToken(string? token) =>
        token?.Trim() is string t && t.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? t[7..].Trim()
            : token?.Trim();

    private static IReadOnlyList<string> NormalizeRoles(IEnumerable<string?> roles) =>
        roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    // Lógica de claims simplificada com Switch Expression
    private static IReadOnlyList<string> TryGetClaimAsStringList(
        Dictionary<string, object?> claims,
        string key
    ) =>
        claims.TryGetValue(key, out var val) && val is not null
            ? val switch
            {
                JsonElement { ValueKind: JsonValueKind.Array } j => NormalizeRoles(
                    j.EnumerateArray().Select(e => e.ToString())
                ),
                JsonElement { ValueKind: JsonValueKind.String } j => NormalizeRoles(
                    [j.GetString()]
                ),
                string s when s.Trim().StartsWith('[') && TryParseRolesJson(s, out var r) => r,
                string s => NormalizeRoles([s]),
                IEnumerable<string> r => NormalizeRoles(r),
                _ => NormalizeRoles([val.ToString()]),
            }
            : [];

    private static bool TryParseRolesJson(string value, out IReadOnlyList<string> roles)
    {
        try
        {
            using var doc = JsonDocument.Parse(value);
            roles =
                doc.RootElement.ValueKind == JsonValueKind.Array
                    ? NormalizeRoles(doc.RootElement.EnumerateArray().Select(e => e.ToString()))
                    : [];
            return roles.Count > 0;
        }
        catch
        {
            roles = [];
            return false;
        }
    }

    public Task AddToBlacklistAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw LogicException.NullValue(nameof(jti));
        }

        return blacklistStore.AddAsync(jti.Trim(), _blacklistTtl);
    }

    public Task<bool> IsBlacklistedAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw LogicException.NullValue(nameof(jti));
        }

        return blacklistStore.IsBlacklistedAsync(jti.Trim());
    }

    private static string GetRequiredClaim(Dictionary<string, object?> claims, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (claims.TryGetValue(key, out object? value) && value is not null)
            {
                string text = value.ToString()?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        throw new UnauthorizedAccessException($"Claim JWT obrigatória ausente: {keys[0]}.");
    }

    private static Guid? TryGetOptionalGuidClaim(Dictionary<string, object?> claims, string key)
    {
        if (!claims.TryGetValue(key, out object? value) || value is null)
        {
            return null;
        }

        string text = value.ToString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return Guid.TryParse(text, out var parsed)
            ? parsed
            : throw new UnauthorizedAccessException($"Claim JWT inválida: {key}.");
    }
}
