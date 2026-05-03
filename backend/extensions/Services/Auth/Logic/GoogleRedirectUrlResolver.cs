namespace backend.extensions.Services.Auth.Logic;

public static class GoogleRedirectUrlResolver
{
    private static readonly string[] TunnelMarkers =
    [
        "ngrok",
        "grok",
        "localtunnel",
        "serveo",
        "pagekite",
        "tunnel",
    ];

    public static string ResolvePreferredFrontendRedirectUrl(IEnumerable<string> allowedOrigins)
    {
        var origins = NormalizeAllowedOrigins(allowedOrigins);

        // Cadeia de prioridade: Local -> Tunnel -> Primeira disponível -> Fallback
        return origins.FirstOrDefault(IsLocalOrigin)
            ?? origins.FirstOrDefault(IsTunnelOrigin)
            ?? origins.FirstOrDefault()
            ?? "/";
    }

    public static string ResolveFrontendRedirectUrl(
        string? requested,
        IEnumerable<string> allowed,
        string fallback
    )
    {
        var allowedOrigins = NormalizeAllowedOrigins(allowed);

        // Verifica se a URL solicitada ou o fallback são válidos e permitidos
        foreach (var url in new[] { requested, fallback }.Select(NormalizeAbsoluteOrigin))
        {
            if (url != null && allowedOrigins.Contains(url, StringComparer.OrdinalIgnoreCase))
                return url;
        }

        return ResolvePreferredFrontendRedirectUrl(allowedOrigins);
    }

    private static string[] NormalizeAllowedOrigins(IEnumerable<string> allowed) =>
        allowed
            .Select(NormalizeAbsoluteOrigin)
            .Where(o => o != null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string? NormalizeAbsoluteOrigin(string? value) =>
        Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.GetLeftPart(UriPartial.Authority).TrimEnd('/')
            : null;

    private static bool IsLocalOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (
            uri.Host is "localhost" or "127.0.0.1" or "::1"
            || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
        );

    private static bool IsTunnelOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && TunnelMarkers.Any(m => uri.Host.Contains(m, StringComparison.OrdinalIgnoreCase));
}
