using System.Net.Http.Headers;
using backend.Core;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Features.Rows.Infrastructure.Extensions;

public static class RowsServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRows(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);
        services.AddTransient<RowsRetryHandler>();

        services
            .AddHttpClient(
                RowsHttpClientDefaults.ClientName,
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<AppSettings>();

                    if (string.IsNullOrWhiteSpace(settings.RowsApiKey))
                        throw new InvalidOperationException("ROWS_API_KEY não definida no .env");

                    client.BaseAddress = BuildBaseAddress(settings.RowsBaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        settings.RowsApiKey
                    );
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                }
            )
            .AddHttpMessageHandler<RowsRetryHandler>();

        services.AddScoped<IRowsService, RowsService>();

        return services;
    }

    private static Uri BuildBaseAddress(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("ROWS_BASE_URL inválida.");

        var normalized = url.Trim();
        if (!normalized.EndsWith('/'))
            normalized += "/";

        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            ? uri
            : throw new InvalidOperationException("Formato de ROWS_BASE_URL inválido.");
    }
}

internal static class RowsHttpClientDefaults
{
    public const string ClientName = "RowsApi";
}
