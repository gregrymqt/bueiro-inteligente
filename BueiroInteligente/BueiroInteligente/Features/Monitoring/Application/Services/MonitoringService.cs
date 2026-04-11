using BueiroInteligente.Core;
using BueiroInteligente.Extensions;
using BueiroInteligente.Features.Monitoring.Application.DTOs;
using BueiroInteligente.Features.Monitoring.Domain.Interfaces;
using BueiroInteligente.Infrastructure.Cache;
using Microsoft.Extensions.Logging;

namespace BueiroInteligente.Features.Monitoring.Application.Services;

/// <summary>
/// Implements the monitoring orchestration, validation, and broadcast logic.
/// </summary>
public sealed class MonitoringService(
    IMonitoringRepository monitoringRepository,
    ICacheService cacheService,
    IRealtimeService realtimeService,
    ILogger<MonitoringService> logger
) : IMonitoringService
{
    private const double MaxBucketDepthCm = 120.0;
    private const double CriticalThreshold = 80.0;
    private const double AlertThreshold = 50.0;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly IMonitoringRepository _monitoringRepository =
        monitoringRepository ?? throw new ArgumentNullException(nameof(monitoringRepository));

    private readonly ICacheService _cacheService =
        cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private readonly IRealtimeService _realtimeService =
        realtimeService ?? throw new ArgumentNullException(nameof(realtimeService));

    private readonly ILogger<MonitoringService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<DrainStatusDTO> ProcessSensorDataAsync(
        SensorPayloadDTO payload,
        CancellationToken cancellationToken = default
    )
    {
        if (payload is null)
        {
            throw LogicException.NullValue(nameof(payload));
        }

        if (string.IsNullOrWhiteSpace(payload.IdBueiro))
        {
            throw LogicException.InvalidValue(nameof(payload.IdBueiro), payload.IdBueiro);
        }

        _logger.LogInformation(
            "Recebendo leitura do sensor para o bueiro {DrainIdentifier}.",
            payload.IdBueiro
        );

        ValidateSensorNoise(payload.IdBueiro, payload.DistanciaCm);

        _logger.LogInformation(
            "Calculando obstrução do bueiro {DrainIdentifier} com base na distância {DistanceCm} cm.",
            payload.IdBueiro,
            payload.DistanciaCm
        );

        double normalizedDistanceCm = Math.Round(payload.DistanciaCm, 2);
        double obstructionLevel = CalculateObstructionLevel(normalizedDistanceCm);
        string status = ResolveStatus(obstructionLevel);

        DrainStatusDTO result = new()
        {
            IdBueiro = payload.IdBueiro,
            DistanciaCm = normalizedDistanceCm,
            NivelObstrucao = Math.Round(obstructionLevel, 2),
            Status = status,
            Latitude = payload.Latitude,
            Longitude = payload.Longitude,
            UltimaAtualizacao = DateTimeOffset.UtcNow,
        };

        _logger.LogInformation(
            "Persistindo leitura processada do bueiro {DrainIdentifier} com status {Status}.",
            result.IdBueiro,
            result.Status
        );

        await _monitoringRepository
            .SaveSensorDataAsync(result, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Persistência concluída para o bueiro {DrainIdentifier}.",
            result.IdBueiro
        );

        if (result.Status is "Alerta" or "Crítico")
        {
            try
            {
                _logger.LogInformation(
                    "Disparando atualização em tempo real para o bueiro {DrainIdentifier}.",
                    result.IdBueiro
                );

                await _realtimeService.BroadcastMonitoringData(result).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Falha ao enviar broadcast SignalR para o bueiro {DrainIdentifier}.",
                    result.IdBueiro
                );
            }
        }

        return result;
    }

    public async Task<DrainStatusDTO> GetDrainStatusAsync(
        string drainIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(drainIdentifier))
        {
            throw LogicException.InvalidValue(nameof(drainIdentifier), drainIdentifier);
        }

        string cacheKey = BuildStatusCacheKey(drainIdentifier);

        _logger.LogInformation("Buscando status do bueiro {DrainIdentifier}.", drainIdentifier);

        CacheResponseDto<DrainStatusDTO> response = await _cacheService
            .GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    _logger.LogDebug(
                        "Cache miss para o bueiro {DrainIdentifier}. Consultando banco de dados.",
                        drainIdentifier
                    );

                    DrainStatusDTO? latestStatus = await _monitoringRepository
                        .GetLatestStatusAsync(drainIdentifier, cancellationToken)
                        .ConfigureAwait(false);

                    if (latestStatus is null)
                    {
                        throw new NotFoundException("Bueiro", drainIdentifier);
                    }

                    return latestStatus;
                },
                CacheTtl
            )
            .ConfigureAwait(false);

        _logger.LogInformation(
            response.FromCache
                ? "Status do bueiro {DrainIdentifier} recuperado do cache."
                : "Status do bueiro {DrainIdentifier} recuperado do banco e armazenado no cache.",
            drainIdentifier
        );

        return response.Data;
    }

    private void ValidateSensorNoise(string drainIdentifier, double distanceCm)
    {
        if (double.IsNaN(distanceCm) || double.IsInfinity(distanceCm) || distanceCm < 0 || distanceCm > MaxBucketDepthCm)
        {
            _logger.LogWarning(
                "Sensor ruidoso detectado: Leitura {DistanceCm} ignorada para o bueiro {DrainIdentifier}.",
                distanceCm,
                drainIdentifier
            );

            throw LogicException.InvalidValue(nameof(distanceCm), distanceCm);
        }
    }

    private static double CalculateObstructionLevel(double distanceCm)
    {
        double occupiedSpaceCm = MaxBucketDepthCm - distanceCm;
        return (occupiedSpaceCm / MaxBucketDepthCm) * 100d;
    }

    private static string ResolveStatus(double obstructionLevel)
    {
        if (obstructionLevel >= CriticalThreshold)
        {
            return "Crítico";
        }

        if (obstructionLevel >= AlertThreshold)
        {
            return "Alerta";
        }

        return "Normal";
    }

    private static string BuildStatusCacheKey(string drainIdentifier)
    {
        return $"bueiro:{drainIdentifier}:status";
    }
}