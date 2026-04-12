using System.Globalization;
using backend.Core;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace backend.Features.Rows.Application.Jobs;

/// <summary>
/// Performs the ETL synchronization from monitoring data to Rows.
/// </summary>
[DisallowConcurrentExecution]
public sealed class RowsSyncJob(
    IServiceScopeFactory serviceScopeFactory,
    AppSettings settings,
    ILogger<RowsSyncJob> logger
) : IJob
{
    private const int ChunkSize = 500;

    private readonly IServiceScopeFactory _serviceScopeFactory =
        serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    private readonly AppSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    private readonly ILogger<RowsSyncJob> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        int totalProcessed = 0;

        _logger.LogInformation("Iniciando rotina de sincronização (ETL) com Rows.com...");

        if (
            string.IsNullOrWhiteSpace(_settings.RowsSpreadsheetId)
            || string.IsNullOrWhiteSpace(_settings.RowsTableId)
        )
        {
            _logger.LogWarning(
                "Configurações do Rows ausentes. A rotina de sincronização foi abortada."
            );
            return;
        }

        try
        {
            while (true)
            {
                await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();

                IMonitoringRepository monitoringRepository = scope.ServiceProvider.GetRequiredService<IMonitoringRepository>();
                IRowsService rowsService = scope.ServiceProvider.GetRequiredService<IRowsService>();

                IReadOnlyList<DrainStatusDTO> unsyncedData = await monitoringRepository
                    .GetUnsyncedDataAsync(ChunkSize, cancellationToken)
                    .ConfigureAwait(false);

                if (unsyncedData.Count == 0)
                {
                    if (totalProcessed == 0)
                    {
                        _logger.LogInformation("Nenhum dado novo para sincronizar com Rows.com.");
                    }

                    break;
                }

                _logger.LogInformation(
                    "Encontrados {Count} registros para sincronizar no lote atual.",
                    unsyncedData.Count
                );

                RowsAppendRequest payload = new(BuildValuesMatrix(unsyncedData));

                await rowsService
                    .AppendDataAsync(
                        _settings.RowsSpreadsheetId,
                        _settings.RowsTableId,
                        payload,
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                IReadOnlyCollection<string> identifiersToMark = unsyncedData
                    .Select(record => record.IdBueiro)
                    .Where(identifier => !string.IsNullOrWhiteSpace(identifier))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();

                await monitoringRepository
                    .MarkAsSyncedAsync(identifiersToMark, cancellationToken)
                    .ConfigureAwait(false);

                totalProcessed += unsyncedData.Count;

                _logger.LogInformation(
                    "Lote sincronizado com sucesso. Total acumulado: {TotalProcessed}.",
                    totalProcessed
                );

                if (unsyncedData.Count < ChunkSize)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "Sincronização com Rows concluída com sucesso. Total de registros processados: {TotalProcessed}.",
                totalProcessed
            );
        }
        catch (ExternalApiException exception)
        {
            _logger.LogError(exception, "Falha na rotina de sincronização com a API Rows.");
        }
        catch (ConnectionException exception)
        {
            _logger.LogError(exception, "Falha de conexão durante a rotina de sincronização com Rows.");
        }
        catch (LogicException exception)
        {
            _logger.LogError(exception, "Erro de validação durante a rotina de sincronização com Rows.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha inesperada na rotina de sincronização com Rows.");
        }
    }

    private static IReadOnlyList<IReadOnlyList<object?>> BuildValuesMatrix(
        IReadOnlyList<DrainStatusDTO> unsyncedData
    )
    {
        List<IReadOnlyList<object?>> valuesMatrix = new(unsyncedData.Count);

        foreach (DrainStatusDTO record in unsyncedData)
        {
            valuesMatrix.Add(
                new object?[]
                {
                    record.IdBueiro,
                    record.DistanciaCm,
                    record.NivelObstrucao,
                    record.Status,
                    record.Latitude,
                    record.Longitude,
                    record.UltimaAtualizacao.ToString("O", CultureInfo.InvariantCulture),
                }
            );
        }

        return valuesMatrix;
    }
}