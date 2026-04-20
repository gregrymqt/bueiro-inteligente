using System.Globalization;
using backend.Core;
using backend.Core.Settings;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace backend.Features.Rows.Application.Jobs;

[DisallowConcurrentExecution]
public sealed class RowsSyncJob(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RowsSettings> settings,
    ILogger<RowsSyncJob> logger
) : IJob
{
    private const int ChunkSize = 500;
    private readonly RowsSettings _settings =
        settings?.Value ?? throw new ArgumentNullException(nameof(settings));

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        int totalProcessed = 0;

        if (
            string.IsNullOrWhiteSpace(_settings.SpreadsheetId)
            || string.IsNullOrWhiteSpace(_settings.TableId)
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
                    .AppendDataAsync(_settings.SpreadsheetId, _settings.TableId, payload, ct)
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
