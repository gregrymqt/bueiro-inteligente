using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Core;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace backend.Features.Rows.Application.Services;

/// <summary>
/// HTTP integration with the Rows API.
/// </summary>
public sealed class RowsService(
    IHttpClientFactory httpClientFactory,
    ILogger<RowsService> logger
) : IRowsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private readonly ILogger<RowsService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> AppendDataAsync(
        string spreadsheetId,
        string tableId,
        RowsAppendRequest payload,
        CancellationToken cancellationToken = default
    )
    {
        ValidateIdentifiers(spreadsheetId, tableId, payload);

        _logger.LogInformation(
            "Iniciando envio para Rows. SpreadsheetId={SpreadsheetId}, TableId={TableId}.",
            spreadsheetId,
            tableId
        );

        HttpClient client = _httpClientFactory.CreateClient(RowsHttpClientDefaults.ClientName);

        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"spreadsheets/{spreadsheetId}/tables/{tableId}/values:append"
        )
        {
            Content = JsonContent.Create(payload, options: JsonOptions),
        };

        try
        {
            using HttpResponseMessage response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            await EnsureSuccessAsync(response, "append_data", cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Lote enviado com sucesso para a tabela Rows {TableId}.",
                tableId
            );

            return true;
        }
        catch (HttpRequestException exception)
        {
            throw new ConnectionException(
                "Rows",
                "Falha de conexão ao enviar o lote para a API Rows.",
                exception
            );
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ConnectionException(
                "Rows",
                "Tempo limite excedido ao enviar o lote para a API Rows.",
                exception
            );
        }
    }

    public async Task<RowsCreateTableResponse> CreateTableAsync(
        string spreadsheetId,
        string pageId,
        RowsCreateTableRequest payload,
        CancellationToken cancellationToken = default
    )
    {
        ValidateIdentifiers(spreadsheetId, pageId, payload);

        _logger.LogInformation(
            "Criando tabela Rows. SpreadsheetId={SpreadsheetId}, PageId={PageId}.",
            spreadsheetId,
            pageId
        );

        HttpClient client = _httpClientFactory.CreateClient(RowsHttpClientDefaults.ClientName);

        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"spreadsheets/{spreadsheetId}/pages/{pageId}/tables"
        )
        {
            Content = JsonContent.Create(payload, options: JsonOptions),
        };

        try
        {
            using HttpResponseMessage response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            await EnsureSuccessAsync(response, "create_table", cancellationToken).ConfigureAwait(false);

            RowsCreateTableResponse? createdTable;

            try
            {
                createdTable = await response.Content
                    .ReadFromJsonAsync<RowsCreateTableResponse>(JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException exception)
            {
                throw new ExternalApiException(
                    "Rows",
                    "A API Rows retornou uma resposta JSON inválida ao criar a tabela.",
                    exception
                );
            }
            catch (NotSupportedException exception)
            {
                throw new ExternalApiException(
                    "Rows",
                    "A API Rows retornou um formato de resposta não suportado ao criar a tabela.",
                    exception
                );
            }

            if (createdTable is null)
            {
                throw new ExternalApiException(
                    "Rows",
                    "A API Rows retornou uma resposta vazia ao criar a tabela."
                );
            }

            return createdTable;
        }
        catch (HttpRequestException exception)
        {
            throw new ConnectionException(
                "Rows",
                "Falha de conexão ao criar a tabela no Rows.",
                exception
            );
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ConnectionException(
                "Rows",
                "Tempo limite excedido ao criar a tabela no Rows.",
                exception
            );
        }
    }

    private static void ValidateIdentifiers(
        string spreadsheetId,
        string tableId,
        RowsAppendRequest payload
    )
    {
        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            throw LogicException.InvalidValue(nameof(spreadsheetId), spreadsheetId);
        }

        if (string.IsNullOrWhiteSpace(tableId))
        {
            throw LogicException.InvalidValue(nameof(tableId), tableId);
        }

        if (payload is null)
        {
            throw LogicException.NullValue(nameof(payload));
        }

        if (payload.Values is null || payload.Values.Count == 0)
        {
            throw LogicException.InvalidValue(nameof(payload.Values), payload.Values);
        }
    }

    private static void ValidateIdentifiers(
        string spreadsheetId,
        string pageId,
        RowsCreateTableRequest payload
    )
    {
        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            throw LogicException.InvalidValue(nameof(spreadsheetId), spreadsheetId);
        }

        if (string.IsNullOrWhiteSpace(pageId))
        {
            throw LogicException.InvalidValue(nameof(pageId), pageId);
        }

        if (payload is null)
        {
            throw LogicException.NullValue(nameof(payload));
        }

        if (string.IsNullOrWhiteSpace(payload.Name))
        {
            throw LogicException.InvalidValue(nameof(payload.Name), payload.Name);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operationName,
        CancellationToken cancellationToken
    )
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string errorBody = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        throw new ExternalApiException(
            "Rows",
            $"A API Rows retornou {(int)response.StatusCode} ({response.ReasonPhrase}) durante '{operationName}': {errorBody}"
        );
    }
}