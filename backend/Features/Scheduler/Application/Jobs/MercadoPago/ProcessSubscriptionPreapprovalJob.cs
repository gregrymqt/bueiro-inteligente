using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPreapprovalJob(
    ILogger<ProcessSubscriptionPreapprovalJob> logger,
    IMercadoPagoSubscriptionService mpSubscriptionService,
    ISubscriptionRepository subscriptionRepository,
    AppDbContext dbContext) : IJob<PaymentNotificationData>
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

        // 3. Atualiza os dados dentro de uma transação para garantir atomicidade
        await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            localSubscription.Status = Enum.Parse<SubscriptionStatus>(mpSubscription.Status, true);
            localSubscription.NextPaymentDate = mpSubscription.NextPaymentDate;
            localSubscription.LastModified = DateTime.UtcNow;

            await subscriptionRepository.UpdateAsync(localSubscription).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);

            logger.LogInformation("Assinatura {Id} atualizada para o status: {Status}", resource.Id,
                localSubscription.Status);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            logger.LogError(ex, "Erro ao atualizar assinatura {Id} no banco local.", resource.Id);
            throw; // Relança para o Hangfire tentar novamente se necessário
        }
    }
}