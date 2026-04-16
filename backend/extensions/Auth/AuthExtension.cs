using System.Text.Json;
using backend.Core;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Models;
using backend.Extensions.Auth.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace backend.Extensions.Auth;

public sealed class AuthExtension : IAuthExtension
{
    private readonly AppSettings _settings;
    private readonly ITokenBlacklistStore _blacklistStore;
    private readonly IPasswordHasher<object> _passwordHasher;
    private readonly ILogger<AuthExtension> _logger;
    private readonly TimeSpan _blacklistTtl;

    public AuthExtension(
        AppSettings settings,
        ITokenBlacklistStore blacklistStore,
        IPasswordHasher<object> passwordHasher,
        ILogger<AuthExtension> logger
    )
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _blacklistStore = blacklistStore ?? throw new ArgumentNullException(nameof(blacklistStore));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _blacklistTtl =
            TimeSpan.FromMinutes(_settings.AccessTokenExpireMinutes) + TimeSpan.FromMinutes(1);
    }

    public Task OpenAsync()
    {
        _logger.LogInformation("Iniciando serviço de autenticação e segurança...");

        if (string.IsNullOrWhiteSpace(_settings.SecretKey) || _settings.SecretKey == "mudar-depois")
        {
            _logger.LogWarning("ALERTA: SECRET_KEY não configurada ou insegura.");
        }

        if (string.IsNullOrWhiteSpace(_settings.HardwareToken))
        {
            _logger.LogWarning("ALERTA: HARDWARE_TOKEN não configurado.");
        }

        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        _logger.LogInformation("Encerrando serviço de autenticação.");
        return Task.CompletedTask;
    }

    public string CreateAccessToken(TokenPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw LogicException.InvalidValue(nameof(payload.Email), payload.Email);
        }

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException("SECRET_KEY não configurada.");
        }

        var now = DateTimeOffset.UtcNow;
        var claims = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["sub"] = payload.Email,
            ["role"] = string.IsNullOrWhiteSpace(payload.Role) ? "User" : payload.Role,
            ["exp"] = now.AddMinutes(_settings.AccessTokenExpireMinutes).ToUnixTimeSeconds(),
            ["iat"] = now.ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString(),
        };

        if (payload.AdditionalClaims is not null)
        {
            foreach (KeyValuePair<string, object?> claim in payload.AdditionalClaims)
            {
                if (!claims.ContainsKey(claim.Key))
                {
                    claims[claim.Key] = claim.Value;
                }
            }
        }

        return JwtTokenHelper.Encode(claims, _settings.SecretKey, _settings.Algorithm);
    }

    public Task<bool> VerifyPasswordAsync(string plainPassword, string hashedPassword)
    {
        bool success =
            _passwordHasher.VerifyHashedPassword(new object(), hashedPassword, plainPassword)
            == PasswordVerificationResult.Success;

        return Task.FromResult(success);
    }

    public Task<string> GetPasswordHashAsync(string password)
    {
        string hash = _passwordHasher.HashPassword(new object(), password);
        return Task.FromResult(hash);
    }

    public Task AddToBlacklistAsync(string jti)
    {
        return _blacklistStore.AddAsync(jti, _blacklistTtl);
    }

    public Task<bool> IsBlacklistedAsync(string jti)
    {
        return _blacklistStore.IsBlacklistedAsync(jti);
    }

    public async Task<UserTokenData> GetCurrentUserAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException("SECRET_KEY não configurada.");
        }

        Dictionary<string, object?> claims = JwtTokenHelper.Decode(
            token,
            _settings.SecretKey,
            _settings.Algorithm
        );

        string email = GetClaimAsString(claims, "sub");
        string jti = GetClaimAsString(claims, "jti");
        IReadOnlyList<string> roles = TryGetClaimAsStringList(claims, "roles");

        if (roles.Count == 0)
        {
            string role = TryGetClaimAsString(claims, "role") ?? "User";
            roles = new[] { role };
        }

        if (await _blacklistStore.IsBlacklistedAsync(jti, cancellationToken).ConfigureAwait(false))
        {
            throw new UnauthorizedAccessException("Token foi revogado.");
        }

        return new UserTokenData(email, roles, jti);
    }

    public string VerifyHardwareToken(string? authorizationToken = null, string? queryToken = null)
    {
        if (string.IsNullOrWhiteSpace(_settings.HardwareToken))
        {
            throw new InvalidOperationException("HARDWARE_TOKEN não configurado.");
        }

        string? finalToken = NormalizeBearerToken(authorizationToken);

        if (string.IsNullOrWhiteSpace(finalToken))
        {
            finalToken = NormalizeBearerToken(queryToken);
        }

        if (string.IsNullOrWhiteSpace(finalToken))
        {
            throw new UnauthorizedAccessException("Hardware inválido ou não autorizado.");
        }

        if (!string.Equals(finalToken, _settings.HardwareToken, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Hardware inválido ou não autorizado.");
        }

        return finalToken;
    }

    public static string? NormalizeBearerToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        string normalizedToken = token.Trim();

        if (normalizedToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedToken[7..].Trim();
        }

        return normalizedToken;
    }

    private static string GetClaimAsString(Dictionary<string, object?> claims, string key)
    {
        if (TryGetClaimAsString(claims, key) is not string value)
        {
            throw new UnauthorizedAccessException("Token JWT inválido.");
        }

        return value;
    }

    private static string? TryGetClaimAsString(Dictionary<string, object?> claims, string key)
    {
        if (!claims.TryGetValue(key, out object? value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string stringValue => stringValue,
            JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String =>
                jsonElement.GetString(),
            _ => value.ToString(),
        };
    }

    private static IReadOnlyList<string> TryGetClaimAsStringList(
        Dictionary<string, object?> claims,
        string key
    )
    {
        if (!claims.TryGetValue(key, out object? value) || value is null)
        {
            return Array.Empty<string>();
        }

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                List<string> roles = new();

                foreach (JsonElement element in jsonElement.EnumerateArray())
                {
                    string? role =
                        element.ValueKind == JsonValueKind.String
                            ? element.GetString()
                            : element.ToString();

                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        roles.Add(role.Trim());
                    }
                }

                return NormalizeRoles(roles);
            }

            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                return NormalizeRoles(new[] { jsonElement.GetString() ?? string.Empty });
            }
        }

        if (value is string stringValue)
        {
            if (TryParseRolesJson(stringValue, out IReadOnlyList<string> roles))
            {
                return roles;
            }

            return NormalizeRoles(new[] { stringValue });
        }

        if (value is IEnumerable<string> stringRoles)
        {
            return NormalizeRoles(stringRoles);
        }

        if (value is IEnumerable<object?> objectRoles)
        {
            List<string> roles = new();

            foreach (object? role in objectRoles)
            {
                string? roleText = role?.ToString();

                if (!string.IsNullOrWhiteSpace(roleText))
                {
                    roles.Add(roleText);
                }
            }

            return NormalizeRoles(roles);
        }

        return NormalizeRoles(new[] { value.ToString() ?? string.Empty });
    }

    private static bool TryParseRolesJson(string value, out IReadOnlyList<string> roles)
    {
        roles = Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmedValue = value.Trim();

        if (!trimmedValue.StartsWith('['))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(trimmedValue);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            List<string> parsedRoles = new();

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                string role = element.ToString();

                if (!string.IsNullOrWhiteSpace(role))
                {
                    parsedRoles.Add(role);
                }
            }

            roles = NormalizeRoles(parsedRoles);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

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
