using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Application.Interfaces;
using backend.Features.Monitoring.Domain.Entities;
using backend.Features.Monitoring.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Features.Monitoring.Application.Services;

public sealed class MonitoringIngestionService(
    IMonitoringRepository monitoringRepository,
    ILogger<MonitoringIngestionService> logger
) : IMonitoringIngestionService
{
    public async Task<bool> SaveSensorDataAsync(DrainStatusDTO data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        logger.LogInformation(
            "Orquestrando ingestão da leitura do bueiro {DrainIdentifier}.",
            data.IdBueiro
        );

        try
        {
            var latestRecord = await monitoringRepository
                .GetLatestStatusAsync(data.IdBueiro, ct)
                .ConfigureAwait(false);

            if (!ShouldPersist(latestRecord, data))
            {
                logger.LogInformation(
                    "Leitura idempotente ignorada para o bueiro {DrainIdentifier}.",
                    data.IdBueiro
                );
                return false;
            }

            var entity = ToEntity(data);
            await monitoringRepository.InsertAsync(entity, ct).ConfigureAwait(false);

            return true;
        }
        catch (DbUpdateException ex) when (IsDuplicateReading(ex))
        {
            logger.LogInformation(
                "Leitura duplicada ignorada no PostgreSQL para o bueiro {DrainIdentifier}.",
                data.IdBueiro
            );
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao salvar leitura do bueiro {DrainIdentifier}. Payload: {@Payload}",
                data.IdBueiro,
                data
            );
            throw;
        }
    }

    private static bool ShouldPersist(DrainStatusDTO? latestRecord, DrainStatusDTO current) =>
        latestRecord is null
        || Math.Abs(latestRecord.DistanciaCm - current.DistanciaCm) > 0.01
        || (current.UltimaAtualizacao - latestRecord.UltimaAtualizacao).TotalMinutes >= 1;

    private static DrainStatus ToEntity(DrainStatusDTO data) =>
        new()
        {
            DrainIdentifier = data.IdBueiro,
            DistanceCm = data.DistanciaCm,
            ObstructionLevel = data.NivelObstrucao,
            Status = data.Status,
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            LastUpdate = data.UltimaAtualizacao,
            SyncedToRows = false,
            DataHash = data.DataHash,
        };

    private static bool IsDuplicateReading(DbUpdateException ex)
    {
        if (ex.InnerException is not PostgresException pgEx || pgEx.SqlState != "23505")
            return false;

        return string.Equals(
                pgEx.ConstraintName,
                "IX_drain_status_data_hash",
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                pgEx.ConstraintName,
                "IX_drain_status_id_bueiro_ultima_atualizacao",
                StringComparison.OrdinalIgnoreCase
            );
    }
}