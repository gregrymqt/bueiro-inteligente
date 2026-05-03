using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace backend.extensions.Services.Auth.Utils;

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

        if (!claims.TryGetValue("exp", out object? expValue)
            || !TryToUnixTimeSeconds(expValue, out long exp)) return claims;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return now >= exp ? throw new UnauthorizedAccessException("Token expirado.") : claims;
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
            case JsonElement { ValueKind: JsonValueKind.Number } jsonElement
                when jsonElement.TryGetInt64(out long parsedLong):
                unixTimeSeconds = parsedLong;
                return true;
            case string stringValue when long.TryParse(stringValue, out long parsedString):
                unixTimeSeconds = parsedString;
                return true;
            default:
                unixTimeSeconds = 0;
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