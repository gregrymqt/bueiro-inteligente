using System.Globalization;
using backend.Core;
using backend.Features.Drains.Domain.Entities;
using backend.Features.Notifications.Application.Interfaces;
using backend.Features.Users.Domain.Entities;
using backend.Features.Users.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace backend.Features.Notifications.Application.Services;

public sealed class PushNotificationService(
    IUserDeviceRepository userDeviceRepository,
    IUnitOfWork unitOfWork,
    ILogger<PushNotificationService> logger
) : IPushNotificationService
{
    public async Task RegisterDeviceAsync(Guid userId, string fcmToken, CancellationToken ct = default)
    {
        if (userId == Guid.Empty) throw new LogicException("ID do usuário inválido.");
        if (string.IsNullOrWhiteSpace(fcmToken)) throw new LogicException("Token FCM não pode ser vazio.");

        var existingDevice = await userDeviceRepository.GetByTokenAsync(fcmToken, ct).ConfigureAwait(false);

        if (existingDevice is null)
        {
            var newDevice = new UserDevice
            {
                UserId = userId,
                FcmToken = fcmToken
            };

            // Usamos o UnitOfWork para garantir que a gravação seja transacionada corretamente
            await unitOfWork.ExecuteTransactionAsync(async transactionCt =>
            {
                await userDeviceRepository.AddAsync(newDevice, transactionCt).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
            
            logger.LogInformation("Dispositivo registrado com sucesso para o usuário {UserId}", userId);
        }
    }

    public async Task SendDrainAlertAsync(string status, double obstructionLevel, Drain bueiro, CancellationToken ct = default)
    {
        try
        {
            var tokens = await userDeviceRepository.GetTokensByUserIdAsync(bueiro.UserId, ct).ConfigureAwait(false);

            if (tokens.Count == 0) return;

            var dataPayload = new Dictionary<string, string>
            {
                { "id", bueiro.Id.ToString() },
                { "name", bueiro.Name },
                { "address", bueiro.Address },
                { "hardware_id", bueiro.HardwareId },
                { "is_active", bueiro.IsActive.ToString().ToLowerInvariant() },
                { "status", status },
                { "nivel_obstrucao", obstructionLevel.ToString("F1", CultureInfo.InvariantCulture) },
                { "distancia_cm", bueiro.MaxHeight.ToString("F1", CultureInfo.InvariantCulture) },
                { "latitude", bueiro.Latitude.ToString(CultureInfo.InvariantCulture) },
                { "longitude", bueiro.Longitude.ToString(CultureInfo.InvariantCulture) },
                { "ultima_atualizacao", DateTimeOffset.UtcNow.ToString("o") }
            };

            var message = new MulticastMessage()
            {
                Tokens = tokens.ToList(),
                Data = dataPayload
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, ct).ConfigureAwait(false);
            logger.LogInformation("Push notifications enviadas. Sucessos: {SuccessCount}, Falhas: {FailureCount}", response.SuccessCount, response.FailureCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao disparar notificações push via Firebase para o bueiro {DrainId}", bueiro.HardwareId);
        }
    }
}