using System.Text.Json;
using backend.Core.Settings;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.App;

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

        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            });

        services.AddHttpContextAccessor();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedHost
                | ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

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
}
