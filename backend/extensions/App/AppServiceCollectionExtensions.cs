using backend.Core.Settings;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace backend.Extensions.App.Middleware;

public static class AppServiceCollectionExtensions
{
    public const string RestrictedOriginsPolicyName = "RestrictedOrigins";

    public static IServiceCollection AddBueiroInteligenteApp(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ICorsPolicyProvider, RestrictedOriginsCorsPolicyProvider>();

        return services;
    }

    public static WebApplication UseBueiroInteligenteApp(this WebApplication app)
    {
        app.UseMiddleware<AppIdMiddleware>();

        return app;
    }

    private sealed class RestrictedOriginsCorsPolicyProvider(
        IOptionsMonitor<GeneralSettings> settingsMonitor
    ) : ICorsPolicyProvider
    {
        public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
        {
            if (!string.Equals(policyName, RestrictedOriginsPolicyName, StringComparison.Ordinal))
            {
                return Task.FromResult<CorsPolicy?>(null);
            }

            var settings = settingsMonitor.CurrentValue;
            var allowedOrigins = settings.AllowedOrigins
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var explicitOrigins = allowedOrigins
                .Where(origin => !string.Equals(origin, "*", StringComparison.Ordinal))
                .ToArray();

            var builder = new CorsPolicyBuilder().AllowAnyHeader().AllowAnyMethod();

            if (explicitOrigins.Length > 0)
            {
                builder.WithOrigins(explicitOrigins).AllowCredentials();
            }
            else
            {
                builder.AllowAnyOrigin();
            }

            return Task.FromResult<CorsPolicy?>(builder.Build());
        }
    }
}
