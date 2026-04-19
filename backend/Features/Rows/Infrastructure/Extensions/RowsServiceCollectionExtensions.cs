using System.Net.Http.Headers;
using backend.Core.Settings;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace backend.Features.Rows.Infrastructure.Extensions;

public static class RowsServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteRows(this IServiceCollection services)
    {
        services.AddTransient<RowsRetryHandler>();

        services
            .AddHttpClient(
                RowsHttpClientDefaults.ClientName,
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<IOptions<RowsSettings>>().Value;

                    if (string.IsNullOrWhiteSpace(settings.ApiKey))
                        throw new InvalidOperationException("ROWS_API_KEY não definida no .env");

                    client.BaseAddress = BuildBaseAddress(settings.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        settings.ApiKey
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
