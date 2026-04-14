namespace backend.Extensions.Auth;

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
        string[] normalizedOrigins = NormalizeAllowedOrigins(allowedOrigins);

        string? preferredOrigin = normalizedOrigins.FirstOrDefault(IsLocalOrigin);
        if (!string.IsNullOrWhiteSpace(preferredOrigin))
        {
            return preferredOrigin;
        }

        preferredOrigin = normalizedOrigins.FirstOrDefault(IsTunnelOrigin);
        if (!string.IsNullOrWhiteSpace(preferredOrigin))
        {
            return preferredOrigin;
        }

        preferredOrigin = normalizedOrigins.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(preferredOrigin))
        {
            return preferredOrigin;
        }

        return "/";
    }

    public static string ResolveFrontendRedirectUrl(
        string? requestedFrontendRedirectUrl,
        IEnumerable<string> allowedOrigins,
        string fallbackFrontendRedirectUrl
    )
    {
        string[] normalizedAllowedOrigins = NormalizeAllowedOrigins(allowedOrigins);

        string? requestedOrigin = NormalizeAbsoluteOrigin(requestedFrontendRedirectUrl);
        if (requestedOrigin is not null && IsAllowedOrigin(requestedOrigin, normalizedAllowedOrigins))
        {
            return requestedOrigin;
        }

        string? fallbackOrigin = NormalizeAbsoluteOrigin(fallbackFrontendRedirectUrl);
        if (fallbackOrigin is not null && IsAllowedOrigin(fallbackOrigin, normalizedAllowedOrigins))
        {
            return fallbackOrigin;
        }

        return ResolvePreferredFrontendRedirectUrl(normalizedAllowedOrigins);
    }

    private static string[] NormalizeAllowedOrigins(IEnumerable<string> allowedOrigins)
    {
        return allowedOrigins
            .Select(NormalizeAbsoluteOrigin)
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsAllowedOrigin(string origin, IEnumerable<string> allowedOrigins)
    {
        return allowedOrigins.Any(allowedOrigin =>
            string.Equals(allowedOrigin, origin, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static string? NormalizeAbsoluteOrigin(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        if (
            !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        )
        {
            return null;
        }

        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    private static bool IsLocalOrigin(string origin)
    {
        return TryGetHost(origin, out string? host)
            && (
                string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static bool IsTunnelOrigin(string origin)
    {
        return TryGetHost(origin, out string? host)
            && TunnelMarkers.Any(marker => host.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryGetHost(string origin, out string host)
    {
        host = string.Empty;

        if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        host = uri.Host;
        return !string.IsNullOrWhiteSpace(host);
    }
}