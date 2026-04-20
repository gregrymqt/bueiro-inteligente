using backend.Core.Settings;
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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var generalSettings =
            configuration.GetSection(GeneralSettings.SectionName).Get<GeneralSettings>()
            ?? new GeneralSettings();

        var allowedOrigins = generalSettings
            .AllowedOrigins.Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var explicitOrigins = allowedOrigins
            .Where(origin => !string.Equals(origin, "*", StringComparison.Ordinal))
            .ToArray();

        services.AddCors(options =>
        {
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
