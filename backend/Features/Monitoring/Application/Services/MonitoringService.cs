using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using backend.Core;
using backend.extensions.Services.Realtime.Abstractions;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Application.Interfaces;
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
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<DrainStatusDTO> ProcessSensorDataAsync(
        SensorPayloadDTO payload,
        CancellationToken ct = default
    )
    {
        logger.LogInformation(
            "Processando leitura para o bueiro {DrainIdentifier}",
            payload?.IdBueiro
        );

        try
        {
            ArgumentNullException.ThrowIfNull(payload);

            if (string.IsNullOrWhiteSpace(payload.IdBueiro))
                throw LogicException.InvalidValue(nameof(payload.IdBueiro), payload.IdBueiro);

            var config = await monitoringRepository
                .GetConfigByIdAsync(payload.IdBueiro, ct)
                .ConfigureAwait(false);

            ValidateSensorNoise(payload.IdBueiro, payload.DistanciaCm, config.MaxHeight);

            double normalizedDistance = Math.Round(payload.DistanciaCm, 2);
            double obstructionLevel = Math.Round(
                CalculateObstructionLevel(normalizedDistance, config.MaxHeight),
                2
            );
            string status = ResolveStatus(
                obstructionLevel,
                config.CriticalThreshold,
                config.AlertThreshold
            );

            DateTimeOffset ultimaAtualizacao = payload.UltimaAtualizacao ?? DateTimeOffset.UtcNow;

            string rawHash =
                $"{payload.IdBueiro}|{normalizedDistance.ToString(CultureInfo.InvariantCulture)}|{ultimaAtualizacao.ToString("O", CultureInfo.InvariantCulture)}";
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawHash));
            string dataHash = Convert.ToHexString(hashBytes);

            var result = new DrainStatusDTO(
                payload.IdBueiro,
                normalizedDistance,
                obstructionLevel,
                status,
                payload.Latitude,
                payload.Longitude,
                ultimaAtualizacao,
                dataHash
            );

            await monitoringRepository.SaveSensorDataAsync(result, ct).ConfigureAwait(false);

            // Disparo condicional de Realtime
            if (result.Status is not ("Alerta" or "Crítico")) return result;
            try
            {
                await realtimeService.PublishAsync("Monitoring_Data", result).ConfigureAwait(false);
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Falha no Realtime Broadcast para o bueiro {Id}. O socket pode estar instável.",
                    result.IdBueiro
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar medição do bueiro {Id}. Payload: {@Payload}",
                payload?.IdBueiro,
                payload
            );
            throw;
        }
    }

    public async Task<DrainStatusDTO> GetDrainStatusAsync(
        string drainId,
        CancellationToken ct = default
    )
    {
        logger.LogInformation("Obtendo status do bueiro {DrainId}", drainId);

        try
        {
            if (string.IsNullOrWhiteSpace(drainId))
                throw LogicException.InvalidValue(nameof(drainId), drainId);

            var response = await cacheService
                .GetOrSetAsync(
                    $"bueiro:{drainId}:status",
                    async () =>
                        await monitoringRepository
                            .GetLatestStatusAsync(drainId, ct)
                            .ConfigureAwait(false)
                        ?? throw new NotFoundException("Bueiro", drainId),
                    CacheTtl
                )
                .ConfigureAwait(false);

            return response.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter status do bueiro {DrainId}.", drainId);
            throw;
        }
    }

    private void ValidateSensorNoise(string id, double distanceCm, double maxHeight)
    {
        if (
            double.IsNaN(distanceCm)
            || double.IsInfinity(distanceCm)
            || distanceCm < 0
            || distanceCm > maxHeight
        )
        {
            logger.LogWarning("Ruído detectado: {DistanceCm} ignorada para {Id}", distanceCm, id);
            throw LogicException.InvalidValue(nameof(distanceCm), distanceCm);
        }
    }

    private static double CalculateObstructionLevel(double dist, double maxHeight) =>
        ((maxHeight - dist) / maxHeight) * 100d;

    private static string ResolveStatus(
        double level,
        double criticalThreshold,
        double alertThreshold
    ) =>
        level switch
        {
            var l when l >= criticalThreshold => "Crítico",
            var l when l >= alertThreshold => "Alerta",
            _ => "Normal",
        };
}