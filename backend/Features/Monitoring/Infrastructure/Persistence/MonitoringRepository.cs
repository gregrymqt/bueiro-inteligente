using backend.Core;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Monitoring.Infrastructure.Persistence;

// C# 12: Injeção direta via Primary Constructor
public sealed class MonitoringRepository(
    AppDbContext dbContext,
    ICacheService cacheService,
    ILogger<MonitoringRepository> logger
) : IMonitoringRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task SaveSensorDataAsync(DrainStatusDTO data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        // 1. Atualização do Cache (Redis)
        try
        {
            await cacheService
                .SetAsync($"bueiro:{data.IdBueiro}:status", data, CacheTtl)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Falha no cache para {Id}. Prosseguindo com persistência.",
                data.IdBueiro
            );
        }

        // 2. Verificação de Idempotência e Persistência Histórica (PostgreSQL)
        try
        {
            var latestRecord = await dbContext
                .DrainStatuses.AsNoTracking()
                .Where(s => s.DrainIdentifier == data.IdBueiro)
                .OrderByDescending(s => s.LastUpdate)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (
                latestRecord is null
                || Math.Abs(latestRecord.DistanceCm - data.DistanciaCm) > 0.01
                || (data.UltimaAtualizacao - latestRecord.LastUpdate).TotalMinutes >= 1
            )
            {
                DrainStatus entity = new()
                {
                    DrainIdentifier = data.IdBueiro,
                    DistanceCm = data.DistanciaCm,
                    ObstructionLevel = data.NivelObstrucao,
                    Status = data.Status,
                    Latitude = data.Latitude,
                    Longitude = data.Longitude,
                    LastUpdate = data.UltimaAtualizacao,
                    SyncedToRows = false,
                    DataHash = data.DataHash
                };

                await dbContext.DrainStatuses.AddAsync(entity, ct).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            else
            {
                logger.LogInformation(
                    "Leitura duplicada ignorada (idempotência) para o bueiro {DrainIdentifier}",
                    data.IdBueiro
                );
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            if (pgEx.ConstraintName == "IX_drain_status_data_hash")
            {
                logger.LogInformation(
                    "Leitura duplicada ignorada (idempotência via Constraint) para o bueiro {DrainIdentifier}",
                    data.IdBueiro
                );
                return;
            }

            logger.LogError(
                ex,
                "Erro de violação de unicidade não esperada no PostgreSQL para {IdBueiro}. Constraint: {Constraint}. Payload: {@Payload}",
                data.IdBueiro,
                pgEx.ConstraintName,
                data
            );
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Erro ao salvar leitura no PostgreSQL para {IdBueiro}. Payload: {@Payload}",
                data.IdBueiro,
                data
            );

            throw new ConnectionException(
                "PostgreSQL",
                $"Erro ao salvar leitura de {data.IdBueiro}",
                ex
            );
        }
    }

    public async Task<DrainStatusDTO?> GetLatestStatusAsync(
        string drainId,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(drainId))
            throw LogicException.InvalidValue(nameof(drainId), drainId);

        try
        {
            var record = await dbContext
                .DrainStatuses.AsNoTracking()
                .Where(s => s.DrainIdentifier == drainId)
                .OrderByDescending(s => s.LastUpdate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            return record is null ? null : MapToDto(record);
        }
        catch (Exception ex)
        {
            throw new ConnectionException("PostgreSQL", $"Erro ao buscar status de {drainId}", ex);
        }
    }

    public async Task<IReadOnlyList<DrainStatusDTO>> GetUnsyncedDataAsync(
        int limit = 100,
        CancellationToken ct = default
    )
    {
        if (limit <= 0)
            throw LogicException.InvalidValue(nameof(limit), limit);

        try
        {
            var records = await dbContext
                .DrainStatuses.AsNoTracking()
                .Where(s => !s.SyncedToRows)
                .OrderBy(s => s.LastUpdate)
                .Take(limit)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return [.. records.Select(MapToDto)]; // C# 12: Collection expression
        }
        catch (Exception ex)
        {
            throw new ConnectionException(
                "PostgreSQL",
                "Erro ao buscar dados não sincronizados.",
                ex
            );
        }
    }

    public async Task<BueiroConfiguration> GetConfigByIdAsync(string id, CancellationToken ct = default)
    {
        var drainConfig = await dbContext.Drains
            .AsNoTracking()
            .Where(d => d.HardwareId == id)
            .Select(d => new BueiroConfiguration
            {
                IdBueiro = d.HardwareId,
                MaxHeight = d.MaxHeight,
                CriticalThreshold = d.CriticalThreshold,
                AlertThreshold = d.AlertThreshold
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return drainConfig ?? new BueiroConfiguration
        {
            IdBueiro = id,
            MaxHeight = 120.0,
            CriticalThreshold = 80.0,
            AlertThreshold = 50.0
        };
    }

    public async Task MarkAsSyncedAsync(
        IReadOnlyCollection<string> drainIds,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(drainIds);
        if (drainIds.Count == 0)
            return;

        try
        {
            var identifiers = drainIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();
            if (identifiers.Length == 0)
                return;

            // Uso do ExecuteUpdateAsync (EF Core 7/8) para performance em lote
            await dbContext
                .DrainStatuses.Where(s =>
                    identifiers.Contains(s.DrainIdentifier) && !s.SyncedToRows
                )
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.SyncedToRows, true), ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ConnectionException("PostgreSQL", "Erro ao marcar sincronização.", ex);
        }
    }

    private static DrainStatusDTO MapToDto(DrainStatus s) =>
        new(
            s.DrainIdentifier,
            s.DistanceCm,
            s.ObstructionLevel,
            s.Status,
            s.Latitude,
            s.Longitude,
            s.LastUpdate,
            s.DataHash
        );
}
