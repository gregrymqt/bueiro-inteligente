using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPreapprovalJob(
    ILogger<ProcessSubscriptionPreapprovalJob> logger,
    IMercadoPagoSubscriptionService mpSubscriptionService,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation("📝 Sincronizando Assinatura (Preapproval): {Id}", resource.Id);

        // 1. Busca os dados atualizados diretamente na API do Mercado Pago
        var mpSubscription = await mpSubscriptionService.GetSubscriptionAsync(resource.Id!).ConfigureAwait(false);

        if (mpSubscription == null)
        {
            logger.LogWarning("Assinatura {Id} não encontrada no Mercado Pago.", resource.Id);
            return;
        }

        // 2. Busca a assinatura correspondente no seu banco de dados local
        var localSubscription = await subscriptionRepository.GetByExternalIdAsync(resource.Id!).ConfigureAwait(false);

        if (localSubscription == null)
        {
            logger.LogError("Assinatura {Id} recebida via Webhook não existe no banco local.", resource.Id);
            return;
        }

        await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            localSubscription.Status = Enum.Parse<SubscriptionStatus>(mpSubscription.Status, true);
            localSubscription.NextPaymentDate = mpSubscription.NextPaymentDate;
            localSubscription.LastModified = DateTime.UtcNow;

            await subscriptionRepository.UpdateAsync(localSubscription).ConfigureAwait(false);
        }).ConfigureAwait(false);

        logger.LogInformation(
            "Assinatura {Id} atualizada para o status: {Status}",
            resource.Id,
            localSubscription.Status
        );
    }
}