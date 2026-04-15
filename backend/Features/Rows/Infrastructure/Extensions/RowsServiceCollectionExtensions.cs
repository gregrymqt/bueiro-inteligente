using System.Net.Http.Headers;
using backend.Core;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Features.Rows.Infrastructure.Extensions;

/// <summary>
/// Registers the Rows API integration and its resilient HttpClient.
/// </summary>
public static class RowsServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRows(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);
        services.AddTransient<RowsRetryHandler>();

        services
            .AddHttpClient(
                RowsHttpClientDefaults.ClientName,
                (serviceProvider, client) =>
                {
                    AppSettings settings = serviceProvider.GetRequiredService<AppSettings>();

                    if (string.IsNullOrWhiteSpace(settings.RowsApiKey))
                    {
                        throw new InvalidOperationException("ROWS_API_KEY não está definida.");
                    }

                    client.BaseAddress = BuildBaseAddress(settings.RowsBaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        settings.RowsApiKey
                    );
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                }
            )
            .AddHttpMessageHandler<RowsRetryHandler>();

        services.AddScoped<IRowsService, RowsService>();

        return services;
    }

    private static Uri BuildBaseAddress(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("ROWS_BASE_URL não está definida.");
        }

        string normalizedBaseUrl = baseUrl.Trim();

        if (!normalizedBaseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            normalizedBaseUrl += "/";
        }

        if (!Uri.TryCreate(normalizedBaseUrl, UriKind.Absolute, out Uri? uri))
        {
            throw new InvalidOperationException("ROWS_BASE_URL possui um formato inválido.");
        }

        return uri;
    }
}

internal static class RowsHttpClientDefaults
{
    public const string ClientName = "RowsApi";
}
