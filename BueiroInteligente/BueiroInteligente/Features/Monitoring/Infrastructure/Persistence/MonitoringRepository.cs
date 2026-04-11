using BueiroInteligente.Core;
using BueiroInteligente.Features.Monitoring.Application.DTOs;
using BueiroInteligente.Features.Monitoring.Domain;
using BueiroInteligente.Features.Monitoring.Domain.Interfaces;
using BueiroInteligente.Infrastructure.Cache;
using BueiroInteligente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BueiroInteligente.Features.Monitoring.Infrastructure.Persistence;

/// <summary>
/// Persists monitoring readings and serves historical drain status data.
/// </summary>
public sealed class MonitoringRepository(
    AppDbContext dbContext,
    ICacheService cacheService,
    ILogger<MonitoringRepository> logger
) : IMonitoringRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly AppDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly ICacheService _cacheService =
        cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private readonly ILogger<MonitoringRepository> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task SaveSensorDataAsync(
        DrainStatusDTO data,
        CancellationToken cancellationToken = default
    )
    {
        if (data is null)
        {
            throw LogicException.NullValue(nameof(data));
        }

        string cacheKey = BuildStatusCacheKey(data.IdBueiro);

        _logger.LogInformation(
            "Atualizando status atual no Redis para o bueiro {DrainIdentifier}.",
            data.IdBueiro
        );

        try
        {
            await _cacheService.SetAsync(cacheKey, data, CacheTtl).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Falha ao atualizar o cache do bueiro {DrainIdentifier}. A persistência histórica continuará.",
                data.IdBueiro
            );
        }

        _logger.LogInformation(
            "Inserindo histórico em drain_status para o bueiro {DrainIdentifier}.",
            data.IdBueiro
        );

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
        };

        try
        {
            await _dbContext.DrainStatuses.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception)
        {
            throw new ConnectionException(
                "PostgreSQL",
                $"Falha ao inserir histórico de medição para o bueiro {data.IdBueiro}.",
                exception
            );
        }

        _logger.LogInformation(
            "Histórico persistido com sucesso para o bueiro {DrainIdentifier}.",
            data.IdBueiro
        );
    }

    public async Task<DrainStatusDTO?> GetLatestStatusAsync(
        string drainIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(drainIdentifier))
        {
            throw LogicException.InvalidValue(nameof(drainIdentifier), drainIdentifier);
        }

        try
        {
            _logger.LogDebug(
                "Consultando o último status do bueiro {DrainIdentifier} no banco de dados.",
                drainIdentifier
            );

            DrainStatus? record = await _dbContext
                .DrainStatuses
                .AsNoTracking()
                .Where(status => status.DrainIdentifier == drainIdentifier)
                .OrderByDescending(status => status.LastUpdate)
                .ThenByDescending(status => status.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (record is null)
            {
                _logger.LogDebug(
                    "Nenhum status encontrado para o bueiro {DrainIdentifier}.",
                    drainIdentifier
                );
                return null;
            }

            return MapToDto(record);
        }
        catch (Exception exception)
        {
            throw new ConnectionException(
                "PostgreSQL",
                $"Falha ao consultar o status do bueiro {drainIdentifier}.",
                exception
            );
        }
    }

    public async Task<IReadOnlyList<DrainStatusDTO>> GetUnsyncedDataAsync(
        int limit = 100,
        CancellationToken cancellationToken = default
    )
    {
        if (limit <= 0)
        {
            throw LogicException.InvalidValue(nameof(limit), limit);
        }

        try
        {
            _logger.LogInformation(
                "Buscando até {Limit} leituras pendentes de sincronização com Rows.",
                limit
            );

            List<DrainStatus> records = await _dbContext
                .DrainStatuses
                .AsNoTracking()
                .Where(status => !status.SyncedToRows)
                .OrderBy(status => status.LastUpdate)
                .Take(limit)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return records.Select(MapToDto).ToList();
        }
        catch (Exception exception)
        {
            throw new ConnectionException(
                "PostgreSQL",
                "Falha ao buscar leituras não sincronizadas com Rows.",
                exception
            );
        }
    }

    public async Task MarkAsSyncedAsync(
        IReadOnlyCollection<string> drainIdentifiers,
        CancellationToken cancellationToken = default
    )
    {
        if (drainIdentifiers is null)
        {
            throw LogicException.NullValue(nameof(drainIdentifiers));
        }

        if (drainIdentifiers.Count == 0)
        {
            return;
        }

        try
        {
            string[] identifiers = drainIdentifiers
                .Where(identifier => !string.IsNullOrWhiteSpace(identifier))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (identifiers.Length == 0)
            {
                return;
            }

            _logger.LogInformation(
                "Marcando {Count} bueiro(s) como sincronizados no banco de dados.",
                identifiers.Length
            );

            await _dbContext
                .DrainStatuses
                .Where(status => identifiers.Contains(status.DrainIdentifier))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(status => status.SyncedToRows, true),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new ConnectionException(
                "PostgreSQL",
                "Falha ao marcar leituras como sincronizadas com Rows.",
                exception
            );
        }
    }

    private static DrainStatusDTO MapToDto(DrainStatus status)
    {
        return new DrainStatusDTO
        {
            IdBueiro = status.DrainIdentifier,
            DistanciaCm = status.DistanceCm,
            NivelObstrucao = status.ObstructionLevel,
            Status = status.Status,
            Latitude = status.Latitude,
            Longitude = status.Longitude,
            UltimaAtualizacao = status.LastUpdate,
        };
    }

    private static string BuildStatusCacheKey(string drainIdentifier)
    {
        return $"bueiro:{drainIdentifier}:status";
    }
}