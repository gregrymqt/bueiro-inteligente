using backend.Core;
using backend.Extensions.Realtime.Abstractions;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Infrastructure.Cache;
using Microsoft.Extensions.Logging;

namespace backend.Features.Monitoring.Application.Services;

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

    public async Task<DrainStatusDTO> ProcessSensorDataAsync(
        SensorPayloadDTO payload,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (string.IsNullOrWhiteSpace(payload.IdBueiro))
            throw LogicException.InvalidValue(nameof(payload.IdBueiro), payload.IdBueiro);

        logger.LogInformation(
            "Processando leitura para o bueiro {DrainIdentifier}",
            payload.IdBueiro
        );

        ValidateSensorNoise(payload.IdBueiro, payload.DistanciaCm);

        double normalizedDistance = Math.Round(payload.DistanciaCm, 2);
        double obstructionLevel = Math.Round(CalculateObstructionLevel(normalizedDistance), 2);
        string status = ResolveStatus(obstructionLevel);

        var result = new DrainStatusDTO(
            payload.IdBueiro,
            normalizedDistance,
            obstructionLevel,
            status,
            payload.Latitude,
            payload.Longitude,
            DateTimeOffset.UtcNow
        );

        await monitoringRepository.SaveSensorDataAsync(result, ct).ConfigureAwait(false);

        // Disparo condicional de Realtime
        if (result.Status is "Alerta" or "Crítico")
        {
            try
            {
                await realtimeService.BroadcastMonitoringData(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha no broadcast SignalR para {Id}", result.IdBueiro);
            }
        }

        return result;
    }

    public async Task<DrainStatusDTO> GetDrainStatusAsync(
        string drainId,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(drainId))
            throw LogicException.InvalidValue(nameof(drainId), drainId);

        var response = await cacheService
            .GetOrSetAsync(
                $"bueiro:{drainId}:status",
                async () =>
                    await monitoringRepository
                        .GetLatestStatusAsync(drainId, ct)
                        .ConfigureAwait(false) ?? throw new NotFoundException("Bueiro", drainId),
                CacheTtl
            )
            .ConfigureAwait(false);

        return response.Data;
    }

    private void ValidateSensorNoise(string id, double dist)
    {
        if (double.IsNaN(dist) || double.IsInfinity(dist) || dist < 0 || dist > MaxBucketDepthCm)
        {
            logger.LogWarning("Ruído detectado: {Dist} ignorada para {Id}", dist, id);
            throw LogicException.InvalidValue(nameof(dist), dist);
        }
    }

    private static double CalculateObstructionLevel(double dist) =>
        ((MaxBucketDepthCm - dist) / MaxBucketDepthCm) * 100d;

    private static string ResolveStatus(double level) =>
        level switch
        {
            >= CriticalThreshold => "Crítico",
            >= AlertThreshold => "Alerta",
            _ => "Normal",
        };
}
