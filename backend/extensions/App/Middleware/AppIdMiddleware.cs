using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace backend.Extensions.App.Middleware;

public sealed class AppIdMiddleware
{
    private const string HeaderName = "X-App-Id";
    private const string AppIdConfigKey = "AppIdSecret";
    private static readonly string[] ExcludedPaths =
    [
        "/health",
        "/api/v1/auth/google-login",
        "/api/v1/auth/google-callback",
        "/api/v1/monitoring/medicoes",
    ];

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppIdMiddleware> _logger;

    public AppIdMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<AppIdMiddleware> logger
    )
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldBypass(context.Request))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        string expectedAppId = _configuration[AppIdConfigKey] ?? string.Empty;
        string providedAppId = context.Request.Headers[HeaderName].ToString();

        if (!string.Equals(providedAppId, expectedAppId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Forbidden request to {Path} with invalid or missing {HeaderName} header.",
                context.Request.Path,
                HeaderName
            );

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            await context
                .Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Forbidden",
                        Detail = "Missing or invalid X-App-Id header.",
                        Status = StatusCodes.Status403Forbidden,
                    }
                )
                .ConfigureAwait(false);

            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private static bool ShouldBypass(HttpRequest request)
    {
        if (HttpMethods.IsOptions(request.Method))
        {
            return true;
        }

        string path = request.Path.Value ?? string.Empty;

        return ExcludedPaths.Any(segments =>
            path.StartsWith(segments, StringComparison.OrdinalIgnoreCase)
        );
    }
}
