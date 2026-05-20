using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using backend.Core;
using backend.extensions.Services.Realtime.Abstractions;
using backend.Features.Drains.Domain.Entities;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Application.Interfaces;
using backend.Features.Monitoring.Domain.Configuration;
using backend.Features.Monitoring.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Monitoring.Application.Services;

public sealed class MonitoringService(
    IMonitoringRepository monitoringRepository,
    IMonitoringIngestionService monitoringIngestionService,
    IRealtimeService _realtimeService,
    ILogger<MonitoringService> logger
) : IMonitoringService
{
    public async Task<DrainStatusDTO> ProcessSensorDataAsync(
        SensorPayloadDTO payload,
        CancellationToken ct = default
    )
    {
        logger.LogInformation("Processando leitura via hardware para o bueiro {DrainIdentifier}", payload?.IdBueiro);

        try
        {
            ArgumentNullException.ThrowIfNull(payload);

            if (string.IsNullOrWhiteSpace(payload.IdBueiro))
                throw LogicException.InvalidValue(nameof(payload.IdBueiro), payload.IdBueiro);

            // 1. Busca isolada via Repository pela coluna HardwareId (Garante Clean Architecture)
            Drain? bueiro = await monitoringRepository
                .GetDrainByHardwareIdAsync(payload.IdBueiro, ct)
                .ConfigureAwait(false);

            if (bueiro is null)
                throw new NotFoundException("Bueiro não cadastrado no catálogo", payload.IdBueiro);

            // 2. Processa e valida os cálculos base do sensor físico
            ValidateSensorNoise(bueiro.HardwareId, payload.DistanciaCm, bueiro.MaxHeight);
            
            double nivel = CalculateObstructionLevel(payload.DistanciaCm, bueiro.MaxHeight);
            string status = ResolveStatus(nivel, bueiro.CriticalThreshold, bueiro.AlertThreshold);
            
            // 3. Executa a montagem do Hash Criptográfico de integridade anti-duplicação
            string hash = CalculateDataHash(bueiro.HardwareId, payload.DistanciaCm, payload.UltimaAtualizacao ?? DateTimeOffset.UtcNow);

            var drainStatusDto = new DrainStatusDTO(
                IdBueiro: bueiro.HardwareId, // Seta na coluna id_bueiro para identificação relacional
                DistanciaCm: payload.DistanciaCm,
                NivelObstrucao: nivel,
                Status: status,
                Latitude: payload.Latitude ?? bueiro.Latitude,
                Longitude: payload.Longitude ?? bueiro.Longitude,
                UltimaAtualizacao: payload.UltimaAtualizacao ?? DateTimeOffset.UtcNow,
                DataHash: hash
            );

            // 4. Persiste de forma transacionada no PostgreSQL
            await monitoringIngestionService.SaveSensorDataAsync(drainStatusDto, ct).ConfigureAwait(false);

            // 5. Broadcast em tempo real via WebSockets (SignalR) para o App e React
            await _realtimeService.PublishToDrainAsync(bueiro.HardwareId, "BUEIRO_STATUS_MUDOU", drainStatusDto).ConfigureAwait(false);

            return drainStatusDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao processar telemetria do bueiro {DrainId}.", payload?.IdBueiro);
            throw;
        }
    }

    public async Task<DrainStatusDTO> GetDrainStatusAsync(
        string drainId,
        CancellationToken ct = default
    )
    {
        logger.LogInformation("Consultando status atual do bueiro {DrainId}", drainId);

        try
        {
            if (string.IsNullOrWhiteSpace(drainId))
                throw LogicException.InvalidValue(nameof(drainId), drainId);

            var status = await monitoringRepository
                .GetLatestStatusAsync(drainId, ct)
                .ConfigureAwait(false);

            // Só lança exceção se o bueiro não existir em NENHUMA tabela do banco de dados
            return status ?? throw new NotFoundException("Bueiro", drainId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter status do bueiro {DrainId}.", drainId);
            throw;
        }
    }

    private void ValidateSensorNoise(string id, double distanceCm, double maxHeight)
    {
        if (!double.IsNaN(distanceCm)
            && !double.IsInfinity(distanceCm)
            && !(distanceCm < 0)
            && !(distanceCm > maxHeight)) return;
        logger.LogWarning("Ruído detectado: {DistanceCm} ignorada para {Id}", distanceCm, id);
        throw LogicException.InvalidValue(nameof(distanceCm), distanceCm);
    }

    private static double CalculateObstructionLevel(double dist, double maxHeight) =>
        ((maxHeight - dist) / maxHeight) * 100d;

    private static string ResolveStatus(double level, double criticalThreshold, double alertThreshold) =>
        level switch
        {
            _ when level >= criticalThreshold => "Crítico",
            _ when level >= alertThreshold => "Alerta",
            _ => "Normal"
        };

    // 🔒 REQUISITO: Montagem do método de criptografia de payload do sensor
    private static string CalculateDataHash(string id, double distanceCm, DateTimeOffset timestamp)
    {
        // Monta uma string única concatenando o ID, a distância com ponto invariante e o timestamp Unix
        string rawInput = $"{id}|{distanceCm.ToString(CultureInfo.InvariantCulture)}|{timestamp.ToUnixTimeSeconds()}";
        
        // Aplica o algoritmo SHA256 padrão
        byte[] inputBytes = Encoding.UTF8.GetBytes(rawInput);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        
        // Retorna em formato hexadecimal string amigável para o banco de dados
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}