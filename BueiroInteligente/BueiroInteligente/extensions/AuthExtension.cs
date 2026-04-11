using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BueiroInteligente.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BueiroInteligente.Extensions;

public sealed record TokenPayload(
    string Email,
    string Role = "User",
    IReadOnlyDictionary<string, object?>? AdditionalClaims = null
);

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

public interface ITokenBlacklistStore
{
    Task AddAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default);
}

public sealed class InMemoryTokenBlacklistStore : ITokenBlacklistStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _entries = new(
        StringComparer.Ordinal
    );

    public Task AddAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(ttl);
        _entries[jti] = expiresAt;
        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(jti, out DateTimeOffset expiresAt))
        {
            return Task.FromResult(false);
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(jti, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}

public interface IUserRoleProvider
{
    Task<string?> GetRoleByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public sealed class AuthExtension
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
        string role = TryGetClaimAsString(claims, "role") ?? "User";

        if (await _blacklistStore.IsBlacklistedAsync(jti, cancellationToken).ConfigureAwait(false))
        {
            throw new UnauthorizedAccessException("Token foi revogado.");
        }

        return new UserTokenData(email, role, jti);
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

    public sealed class RoleChecker
    {
        private readonly HashSet<string> _allowedRoles;
        private readonly bool _strictDbCheck;
        private readonly IUserRoleProvider? _roleProvider;

        public RoleChecker(
            IEnumerable<string> allowedRoles,
            bool strictDbCheck = false,
            IUserRoleProvider? roleProvider = null
        )
        {
            if (allowedRoles is null)
            {
                throw new ArgumentNullException(nameof(allowedRoles));
            }

            _allowedRoles = new HashSet<string>(allowedRoles, StringComparer.OrdinalIgnoreCase);
            _strictDbCheck = strictDbCheck;
            _roleProvider = roleProvider;
        }

        public async Task<UserTokenData> AuthorizeAsync(
            UserTokenData currentUser,
            CancellationToken cancellationToken = default
        )
        {
            if (currentUser is null)
            {
                throw LogicException.NullValue(nameof(currentUser));
            }

            string userRole = currentUser.Role;

            if (_strictDbCheck)
            {
                if (_roleProvider is null)
                {
                    throw new InvalidOperationException(
                        "strictDbCheck foi habilitado, mas nenhum IUserRoleProvider foi registrado."
                    );
                }

                string? freshRole = await _roleProvider
                    .GetRoleByEmailAsync(currentUser.Email, cancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(freshRole))
                {
                    throw new UnauthorizedAccessException(
                        "Usuário ou role não encontrados no sistema."
                    );
                }

                userRole = freshRole;
                currentUser.Role = freshRole;
            }

            if (!_allowedRoles.Contains(userRole))
            {
                string allowedRolesText = string.Join(", ", _allowedRoles);

                throw new UnauthorizedAccessException(
                    $"Acesso negado: este recurso exige uma das roles [{allowedRolesText}], mas você possui a role '{userRole}'."
                );
            }

            return currentUser;
        }
    }
}

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteAuth(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);
        services.AddSingleton<ITokenBlacklistStore, InMemoryTokenBlacklistStore>();
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
        services.AddSingleton<AuthExtension>();
        services.AddAuthorization();
        return services;
    }

    public static void InitializeBueiroInteligenteAuth(this IServiceProvider serviceProvider)
    {
        AuthExtension authExtension = serviceProvider.GetRequiredService<AuthExtension>();
        authExtension.OpenAsync().GetAwaiter().GetResult();
    }
}

internal static class JwtTokenHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );

    public static string Encode(
        Dictionary<string, object?> claims,
        string secretKey,
        string algorithm
    )
    {
        EnsureSupportedAlgorithm(algorithm);

        string headerJson = JsonSerializer.Serialize(
            new Dictionary<string, object?> { ["alg"] = "HS256", ["typ"] = "JWT" },
            SerializerOptions
        );

        string payloadJson = JsonSerializer.Serialize(claims, SerializerOptions);
        string headerPart = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        string payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        string signingInput = $"{headerPart}.{payloadPart}";
        string signaturePart = Base64UrlEncode(Sign(signingInput, secretKey));

        return $"{signingInput}.{signaturePart}";
    }

    public static Dictionary<string, object?> Decode(
        string token,
        string secretKey,
        string algorithm
    )
    {
        EnsureSupportedAlgorithm(algorithm);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Token JWT vazio.");
        }

        string[] parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            throw new UnauthorizedAccessException("Token JWT inválido.");
        }

        string signingInput = $"{parts[0]}.{parts[1]}";
        byte[] expectedSignature = Sign(signingInput, secretKey);
        byte[] actualSignature = Base64UrlDecode(parts[2]);

        if (
            expectedSignature.Length != actualSignature.Length
            || !CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature)
        )
        {
            throw new UnauthorizedAccessException("Assinatura do token inválida.");
        }

        string payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
        Dictionary<string, object?> claims = ParseClaims(payloadJson);

        if (
            claims.TryGetValue("exp", out object? expValue)
            && TryToUnixTimeSeconds(expValue, out long exp)
        )
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (now >= exp)
            {
                throw new UnauthorizedAccessException("Token expirado.");
            }
        }

        return claims;
    }

    private static void EnsureSupportedAlgorithm(string algorithm)
    {
        if (!string.Equals(algorithm, "HS256", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Algoritmo JWT '{algorithm}' não suportado nesta implementação. Use HS256."
            );
        }
    }

    private static byte[] Sign(string input, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    private static Dictionary<string, object?> ParseClaims(string payloadJson)
    {
        using JsonDocument document = JsonDocument.Parse(payloadJson);
        var claims = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            claims[property.Name] = ParseJsonValue(property.Value);
        }

        return claims;
    }

    private static object? ParseJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out long int64Value) => int64Value,
            JsonValueKind.Number when element.TryGetDecimal(out decimal decimalValue) =>
                decimalValue,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => element.GetRawText(),
        };
    }

    private static bool TryToUnixTimeSeconds(object? value, out long unixTimeSeconds)
    {
        switch (value)
        {
            case long longValue:
                unixTimeSeconds = longValue;
                return true;
            case int intValue:
                unixTimeSeconds = intValue;
                return true;
            case decimal decimalValue:
                unixTimeSeconds = (long)decimalValue;
                return true;
            case double doubleValue:
                unixTimeSeconds = (long)doubleValue;
                return true;
            case JsonElement jsonElement
                when jsonElement.ValueKind == JsonValueKind.Number
                    && jsonElement.TryGetInt64(out long parsedLong):
                unixTimeSeconds = parsedLong;
                return true;
            case string stringValue when long.TryParse(stringValue, out long parsedString):
                unixTimeSeconds = parsedString;
                return true;
            default:
                unixTimeSeconds = default;
                return false;
        }
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');

        base64 += (base64.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            0 => string.Empty,
            _ => throw new FormatException("Base64Url inválido."),
        };

        return Convert.FromBase64String(base64);
    }
}
