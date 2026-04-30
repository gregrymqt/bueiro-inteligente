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
        logger.LogInformation("Enviando dados para Rows: Tabela {TableId}", tableId);

        try
        {
            Validate(spreadsheetId, tableId, payload);

            var client = httpClientFactory.CreateClient(RowsHttpClientDefaults.ClientName);
            var url = $"v1/spreadsheets/{spreadsheetId}/tables/{tableId}/values/A:G:append";

            using var response = await client
                .PostAsJsonAsync(url, payload, JsonOptions, ct)
                .ConfigureAwait(false);
            await EnsureSuccessAsync(response, "append_data", ct).ConfigureAwait(false);

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Erro ao enviar dados para Rows. SpreadsheetId: {SpreadsheetId}. TableId: {TableId}. Payload: {@Payload}",
                spreadsheetId,
                tableId,
                payload
            );
            throw new ConnectionException("Rows", "Falha de conexão com a API Rows.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao enviar dados para Rows. SpreadsheetId: {SpreadsheetId}. TableId: {TableId}. Payload: {@Payload}",
                spreadsheetId,
                tableId,
                payload
            );
            throw;
        }
    }

    public async Task<RowsCreateTableResponse> CreateTableAsync(
        string spreadsheetId,
        string pageId,
        RowsCreateTableRequest payload,
        CancellationToken ct = default
    )
    {
        logger.LogInformation("Criando tabela no Rows: {Name}", payload?.Name);

        try
        {
            Validate(spreadsheetId, pageId, payload);

            var client = httpClientFactory.CreateClient(RowsHttpClientDefaults.ClientName);
            var url = $"spreadsheets/{spreadsheetId}/pages/{pageId}/tables";

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
            logger.LogError(
                ex,
                "Erro ao criar tabela no Rows. SpreadsheetId: {SpreadsheetId}. PageId: {PageId}. Payload: {@Payload}",
                spreadsheetId,
                pageId,
                payload
            );
            throw new ConnectionException("Rows", "Erro ao criar tabela no Rows.", ex);
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "JSON inválido retornado pelo Rows. SpreadsheetId: {SpreadsheetId}. PageId: {PageId}. Payload: {@Payload}",
                spreadsheetId,
                pageId,
                payload
            );
            throw new ExternalApiException("Rows", "JSON inválido da API Rows.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao criar tabela no Rows. SpreadsheetId: {SpreadsheetId}. PageId: {PageId}. Payload: {@Payload}",
                spreadsheetId,
                pageId,
                payload
            );
            throw;
        }
    }

    #region Validações Enxutas

    private static void Validate(string sid, string targetId, object? payload)
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
