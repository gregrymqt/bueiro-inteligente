using backend.Core;
using backend.Features.Drains.Domain.Entities;
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
    }
    // Adicione esta assinatura na interface IMonitoringRepository se houver, 
    // e implemente o método abaixo na classe MonitoringRepository.cs:
    public async Task<Drain?> GetDrainByHardwareIdAsync(
        string hardwareId,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(hardwareId))
            throw LogicException.InvalidValue(nameof(hardwareId), hardwareId);

        try
        {
            return await dbContext.Drains
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.HardwareId == hardwareId, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ConnectionException(
                "PostgreSQL",
                $"Erro ao buscar bueiro pelo HardwareId: {hardwareId}",
                ex
            );
        }
    }

    public async Task<DrainStatusDTO?> GetLatestStatusAsync(string drainId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(drainId))
            throw LogicException.InvalidValue(nameof(drainId), drainId);

        try
        {
            // 1. HIGIENIZAÇÃO: Remove espaços fantasmas sem forçar caixa alta destrutiva
            var cleanDrainId = drainId.Trim();
            var isGuid = Guid.TryParse(cleanDrainId, out var parsedGuid);

            // 2. Localiza o bueiro no catálogo de forma resiliente
            Drain? drain;
            if (isGuid)
            {
                drain = await dbContext.Drains
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == parsedGuid, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                // Usa ILike nativo do Postgres + Trim na coluna para neutralizar collations e paddings de CHAR fixo
                drain = await dbContext.Drains
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => EF.Functions.ILike(d.HardwareId.Trim(), cleanDrainId), ct)
                    .ConfigureAwait(false);
            }

            if (drain is null) return null;

            // 3. Busca a telemetria mais recente para este bueiro usando o HardwareId correto
            var latestStatus = await dbContext.DrainStatuses.AsNoTracking()
                .Where(s => s.DrainIdentifier == drain.HardwareId)
                .OrderByDescending(s => s.LastUpdate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            // 4. TRATAMENTO DE COLD START: Existe no catálogo mas não tem telemetria ainda (Injeta os metadados!)
            if (latestStatus is null)
            {
                logger.LogInformation("Bueiro {DrainId} localizado no catálogo. Sem telemetria, gerando payload inicial.", drainId);
                return new DrainStatusDTO(
                    IdBueiro: drain.HardwareId,
                    DistanciaCm: drain.MaxHeight,
                    NivelObstrucao: 0.0,
                    Status: "Normal",
                    Latitude: drain.Latitude,
                    Longitude: drain.Longitude,
                    UltimaAtualizacao: DateTimeOffset.UtcNow,
                    DataHash: string.Empty,
                    Id: drain.Id,
                    Name: drain.Name,
                    Address: drain.Address
                );
            }

            // 5. MAPEAR UNIFICANDO TELEMETRIA + CADASTRO (Resolve o Bug do Front/App)
            return new DrainStatusDTO(
                IdBueiro: latestStatus.DrainIdentifier,
                DistanciaCm: latestStatus.DistanceCm,
                NivelObstrucao: latestStatus.ObstructionLevel,
                Status: latestStatus.Status,
                Latitude: latestStatus.Latitude ?? drain.Latitude,
                Longitude: latestStatus.Longitude ?? drain.Longitude,
                UltimaAtualizacao: latestStatus.LastUpdate,
                DataHash: latestStatus.DataHash,
                Id: drain.Id,
                Name: drain.Name,
                Address: drain.Address
            );
        }
        catch (Exception ex)
        {
            throw new ConnectionException("PostgreSQL", $"Erro ao buscar status para o bueiro {drainId}", ex);
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
