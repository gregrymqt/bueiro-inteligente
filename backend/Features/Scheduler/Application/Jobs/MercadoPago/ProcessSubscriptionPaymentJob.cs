using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.extensions.Services.Realtime.Abstractions;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPaymentJob(
    IMercadoPagoPaymentService mpService,
    ISubscriptionRepository repository,
    IRealtimeService realtime,
    ILogger<ProcessSubscriptionPaymentJob> logger
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation("💳 Processando renovação/pagamento de Assinatura: {Id}", resource.Id);

        if (string.IsNullOrEmpty(resource.Id)) return;

        // 1. Busca detalhes do pagamento no Mercado Pago
        var payment = await mpService.GetPaymentAsync(resource.Id);

        if (payment is not { Status: "approved" })
        {
            logger.LogWarning("Pagamento {Id} não aprovado ou não encontrado. Status: {Status}",
                resource.Id, payment?.Status ?? "N/A");
            return;
        }

        try
        {
            // 2. Utiliza o Repositório para buscar a assinatura pela referência do MP
            // O repositório já resolve a busca por ExternalId de forma limpa
            var subscription = await repository.GetByExternalIdAsync(payment.ExternalReference);

            // Fallback: se o ExternalReference no MP for na verdade o ID interno (Guid)
            if (subscription == null && Guid.TryParse(payment.ExternalReference, out Guid internalId))
            {
                subscription = await repository.GetByIdAsync(internalId);
            }

            if (subscription == null)
            {
                logger.LogError("Assinatura não encontrada localmente para a referência: {Ref}", payment.ExternalReference);
                return;
            }

            // 3. Atualiza o status e a data de vencimento
            subscription.Status = SubscriptionStatus.Authorized;
            subscription.NextPaymentDate = (payment.DateApproved?.UtcDateTime ?? DateTime.UtcNow).AddDays(30);

            // 4. Persistência e Invalidação de Cache
            // O UpdateAsync já cuida do SaveChangesAsync e limpa o Redis!
            await repository.UpdateAsync(subscription);

            // 5. Feedback Realtime (SignalR)
            await realtime.PublishToUserAsync(
                subscription.UserId.ToString(),
                "payment_authorized",
                new
                {
                    status = "success",
                    subscription_id = subscription.Id,
                    next_billing = subscription.NextPaymentDate
                }
            );

            logger.LogInformation("✅ Assinatura {SubId} autorizada com sucesso.", subscription.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro crítico ao processar Job de pagamento {Id}", resource.Id);
            throw; // Reenfileira no Hangfire
        }
    }
}