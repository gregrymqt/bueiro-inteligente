using backend.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.App.Middleware;

public static class AppServiceCollectionExtensions
{
    public const string RestrictedOriginsPolicyName = "RestrictedOrigins";

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
            string[] allowedOrigins =
            [
                .. AppSettings
                    .Current.AllowedOrigins.Where(origin => !string.IsNullOrWhiteSpace(origin))
                    .Select(origin => origin.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase),
            ];

            string[] explicitOrigins =
            [
                .. allowedOrigins.Where(origin =>
                    !string.Equals(origin, "*", StringComparison.Ordinal)
                ),
            ];

            options.AddPolicy(
                RestrictedOriginsPolicyName,
                policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();

                    if (explicitOrigins.Length > 0)
                    {
                        policy.WithOrigins(explicitOrigins).AllowCredentials();
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                    }
                }
            );
        });

        return services;
    }

    public static WebApplication UseBueiroInteligenteApp(this WebApplication app)
    {
        app.UseMiddleware<AppIdMiddleware>();

        return app;
    }
}
