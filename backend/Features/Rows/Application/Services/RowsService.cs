using System.Net.Http.Json;
using System.Text.Json;
using backend.Core;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace backend.Features.Rows.Application.Services;

public sealed class RowsService(IHttpClientFactory httpClientFactory, ILogger<RowsService> logger)
    : IRowsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> AppendDataAsync(
        string spreadsheetId,
        string tableId,
        RowsAppendRequest payload,
        CancellationToken ct = default
    )
    {
        Validate(spreadsheetId, tableId, payload);
        logger.LogInformation("Enviando dados para Rows: Tabela {TableId}", tableId);

        var client = httpClientFactory.CreateClient(RowsHttpClientDefaults.ClientName);
        var url = $"spreadsheets/{spreadsheetId}/tables/{tableId}/values:append";

        try
        {
            using var response = await client
                .PostAsJsonAsync(url, payload, JsonOptions, ct)
                .ConfigureAwait(false);
            await EnsureSuccessAsync(response, "append_data", ct).ConfigureAwait(false);

            return true;
        }
        catch (HttpRequestException ex)
        {
            throw new ConnectionException("Rows", "Falha de conexão com a API Rows.", ex);
        }
    }

    public async Task<RowsCreateTableResponse> CreateTableAsync(
        string spreadsheetId,
        string pageId,
        RowsCreateTableRequest payload,
        CancellationToken ct = default
    )
    {
        Validate(spreadsheetId, pageId, payload);
        logger.LogInformation("Criando tabela no Rows: {Name}", payload.Name);

        var client = httpClientFactory.CreateClient(RowsHttpClientDefaults.ClientName);
        var url = $"spreadsheets/{spreadsheetId}/pages/{pageId}/tables";

        try
        {
            using var response = await client
                .PostAsJsonAsync(url, payload, JsonOptions, ct)
                .ConfigureAwait(false);
            await EnsureSuccessAsync(response, "create_table", ct).ConfigureAwait(false);

            return await response
                    .Content.ReadFromJsonAsync<RowsCreateTableResponse>(JsonOptions, ct)
                    .ConfigureAwait(false)
                ?? throw new ExternalApiException("Rows", "Resposta vazia ao criar tabela.");
        }
        catch (HttpRequestException ex)
        {
            throw new ConnectionException("Rows", "Erro ao criar tabela no Rows.", ex);
        }
        catch (JsonException ex)
        {
            throw new ExternalApiException("Rows", "JSON inválido da API Rows.", ex);
        }
    }

    #region Validações Enxutas

    private static void Validate(string sid, string targetId, object payload)
    {
        if (string.IsNullOrWhiteSpace(sid))
            throw LogicException.InvalidValue("spreadsheetId", sid);
        if (string.IsNullOrWhiteSpace(targetId))
            throw LogicException.InvalidValue("targetId", targetId);
        ArgumentNullException.ThrowIfNull(payload);

        if (payload is RowsAppendRequest { Values: null or { Count: 0 } })
            throw LogicException.InvalidValue("Values", "O lote de valores não pode estar vazio.");

        if (payload is RowsCreateTableRequest { Name: var name } && string.IsNullOrWhiteSpace(name))
            throw LogicException.InvalidValue("Name", name);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage res,
        string op,
        CancellationToken ct
    )
    {
        if (res.IsSuccessStatusCode)
            return;
        var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        throw new ExternalApiException("Rows", $"Erro {(int)res.StatusCode} em '{op}': {body}");
    }

    #endregion
}
