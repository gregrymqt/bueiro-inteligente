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

[DisallowConcurrentExecution]
public sealed class RowsSyncJob(
    IServiceScopeFactory serviceScopeFactory,
    AppSettings settings,
    ILogger<RowsSyncJob> logger
) : IJob
{
    private const int ChunkSize = 500;

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        int totalProcessed = 0;

        if (
            string.IsNullOrWhiteSpace(settings.RowsSpreadsheetId)
            || string.IsNullOrWhiteSpace(settings.RowsTableId)
        )
        {
            logger.LogWarning("Configurações do Rows ausentes. Sincronização abortada.");
            return;
        }

        logger.LogInformation("Iniciando ETL com Rows.com...");

        try
        {
            while (true)
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var monitoringRepo =
                    scope.ServiceProvider.GetRequiredService<IMonitoringRepository>();
                var rowsService = scope.ServiceProvider.GetRequiredService<IRowsService>();

                var unsyncedData = await monitoringRepo
                    .GetUnsyncedDataAsync(ChunkSize, ct)
                    .ConfigureAwait(false);

                if (unsyncedData.Count == 0)
                    break;

                // Transformação e Envio
                var payload = new RowsAppendRequest(BuildValuesMatrix(unsyncedData));
                await rowsService
                    .AppendDataAsync(settings.RowsSpreadsheetId, settings.RowsTableId, payload, ct)
                    .ConfigureAwait(false);

                // Marcação de Sincronismo
                var ids = unsyncedData
                    .Select(r => r.IdBueiro)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToArray();

                await monitoringRepo.MarkAsSyncedAsync(ids, ct).ConfigureAwait(false);

                totalProcessed += unsyncedData.Count;
                if (unsyncedData.Count < ChunkSize)
                    break;
            }

            if (totalProcessed > 0)
                logger.LogInformation("Sincronização concluída. Total: {Total}", totalProcessed);
        }
        catch (Exception ex)
            when (ex is ExternalApiException or ConnectionException or LogicException)
        {
            logger.LogError(ex, "Erro controlado na sincronização com Rows.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha inesperada no RowsSyncJob.");
        }
    }

    private static IReadOnlyList<IReadOnlyList<object?>> BuildValuesMatrix(
        IReadOnlyList<DrainStatusDTO> data
    ) =>
        [
            .. data.Select(r =>
                (IReadOnlyList<object?>)
                    [
                        r.IdBueiro,
                        r.DistanciaCm,
                        r.NivelObstrucao,
                        r.Status,
                        r.Latitude,
                        r.Longitude,
                        r.UltimaAtualizacao.ToString("O", CultureInfo.InvariantCulture),
                    ]
            ),
        ];
}
