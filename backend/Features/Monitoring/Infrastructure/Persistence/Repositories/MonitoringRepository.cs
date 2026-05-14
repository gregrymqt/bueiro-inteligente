using backend.Core;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Configuration;
using backend.Features.Monitoring.Domain.Entities;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Monitoring.Infrastructure.Persistence.Repositories;

// C# 12: Injeção direta via Primary Constructor
public sealed class MonitoringRepository(
    AppDbContext dbContext,
    ILogger<MonitoringRepository> logger
) : IMonitoringRepository
{
    public async Task InsertAsync(DrainStatus entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        logger.LogInformation(
            "Inserindo leitura do bueiro {DrainIdentifier} em {LastUpdate}.",
            entity.DrainIdentifier,
            entity.LastUpdate
        );

        await dbContext.DrainStatuses.AddAsync(entity, ct).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
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

    public async Task<BueiroConfiguration?> GetConfigByIdAsync(
        string id,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(id))
            throw LogicException.InvalidValue(nameof(id), id);

        var drainConfig = await dbContext
            .Drains.AsNoTracking()
            .Where(d => d.HardwareId == id)
            .Select(d => new BueiroConfiguration
            {
                IdBueiro = d.HardwareId,
                MaxHeight = d.MaxHeight,
                CriticalThreshold = d.CriticalThreshold,
                AlertThreshold = d.AlertThreshold,
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return drainConfig;
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
