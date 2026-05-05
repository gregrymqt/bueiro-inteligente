using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Features.Notifications.Application;
using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces; // <-- ADICIONADO
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPaymentJob(
    IMercadoPagoPaymentService mpService,
    ISubscriptionRepository repository,
    INotificationService notificationService, // <-- INJEÇÃO DO NOVO SERVIÇO
    ILogger<ProcessSubscriptionPaymentJob> logger
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation("💳 Processando renovação/pagamento de Assinatura: {Id}", resource.Id);

        if (string.IsNullOrEmpty(resource.Id)) return;

        var payment = await mpService.GetPaymentAsync(resource.Id);

        if (payment is not { Status: "approved" })
        {
            logger.LogWarning("Pagamento {Id} não aprovado ou não encontrado. Status: {Status}",
                resource.Id, payment?.Status ?? "N/A");
            return;
        }

        try
        {
            var subscription = await repository.GetByExternalIdAsync(payment.ExternalReference);

            if (subscription == null && Guid.TryParse(payment.ExternalReference, out Guid internalId))
            {
                subscription = await repository.GetByIdAsync(internalId);
            }

            if (subscription == null)
            {
                logger.LogError("Assinatura não encontrada localmente para a referência: {Ref}",
                    payment.ExternalReference);
                return;
            }

            subscription.Status = SubscriptionStatus.Authorized;
            subscription.NextPaymentDate = (payment.DateApproved?.UtcDateTime ?? DateTime.UtcNow).AddDays(30);

            await repository.UpdateAsync(subscription);

            // --- NOVA ABORDAGEM DE NOTIFICAÇÃO ---
            await notificationService.SendNotificationAsync(
                subscription.UserId,
                "Assinatura Renovada",
                $"Sua assinatura foi renovada com sucesso! O próximo faturamento será em {subscription.NextPaymentDate:dd/MM/yyyy}.",
                NotificationType.Success
            );
            // -------------------------------------

            logger.LogInformation("✅ Assinatura {SubId} autorizada com sucesso.", subscription.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro crítico ao processar Job de pagamento {Id}", resource.Id);
            throw;
        }
    }
}