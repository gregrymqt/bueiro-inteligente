using backend.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.App.Middleware;

public static class AppServiceCollectionExtensions
{
    private const string RestrictedOriginsPolicyName = "RestrictedOrigins";

    public static IServiceCollection AddBueiroInteligenteApp(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AppSettings settings = AppSettings.Current;

        configuration["AllowedHosts"] = settings.AllowedHosts;
        configuration["AppIdSecret"] = settings.AppIdSecret;

        services.AddCors(options =>
        {
            string[] allowedOrigins = settings
                .AllowedOrigins.Where(origin =>
                    !string.IsNullOrWhiteSpace(origin)
                    && !string.Equals(origin.Trim(), "*", StringComparison.Ordinal)
                )
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            options.AddPolicy(
                RestrictedOriginsPolicyName,
                policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins);
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                    }

                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                }
            );
        });

        return services;
    }

    public static WebApplication UseBueiroInteligenteApp(this WebApplication app)
    {
        app.UseCors(RestrictedOriginsPolicyName);
        app.UseMiddleware<AppIdMiddleware>();

        return app;
    }
}
