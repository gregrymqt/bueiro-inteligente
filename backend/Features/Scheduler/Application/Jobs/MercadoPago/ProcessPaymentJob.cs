using backend.extensions.Services.Realtime.Abstractions;
using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces; // Namespace do Enum
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessPaymentJob(
    ILogger<ProcessPaymentJob> logger,
    IMercadoPagoPaymentService mpPaymentService,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository, // Injeção do repositório de assinaturas
    AppDbContext dbContext,
    ICacheService cacheService,
    IRealtimeService realtimeService
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation("🚀 Processando Pagamento MP: {PaymentId}", resource.Id);

        if (string.IsNullOrEmpty(resource.Id)) return;

        // 1. Consulta o status atualizado na API do Mercado Pago[cite: 21, 23]
        var mpPaymentInfo = await mpPaymentService.GetPaymentAsync(resource.Id) ??
                            throw new Exception("Pagamento não encontrado na API do Mercado Pago.");

        if (!Guid.TryParse(mpPaymentInfo.ExternalReference, out Guid transactionId)) return;

        // 2. Inicia Transação Atômica[cite: 21]
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var localTransaction = await paymentRepository.GetByIdAsync(transactionId);
            if (localTransaction == null) return;

            // Evita reprocessamento se o status já estiver atualizado[cite: 21]
            if (localTransaction.Status == mpPaymentInfo.Status) return;

            // 3. Atualiza a transação de pagamento[cite: 22]
            long.TryParse(resource.Id, out long paymentIdLong);
            localTransaction.UpdateStatus(mpPaymentInfo.Status, mpPaymentInfo.StatusDetail, paymentIdLong);
            await paymentRepository.UpdateAsync(localTransaction);

            // 4. Lógica de Liberação da Assinatura
            if (mpPaymentInfo.Status == "approved")
            {
                logger.LogInformation("✅ Pagamento aprovado. Ativando assinatura do usuário {UserId}...",
                    localTransaction.UserId);

                await realtimeService.PublishToUserAsync(
                    localTransaction.UserId.ToString(),
                    "PAYMENT_AUTHORIZED",
                    new { transaction_id = localTransaction.Id, status = "success" }
                );

                // Busca a assinatura vinculada ao usuário[cite: 4]
                var cacheSub = await subscriptionRepository.GetByUserIdAsync(localTransaction.UserId);
                var subscription = cacheSub.Data;

                if (subscription != null)
                {
                    // Atualiza para o status autorizado conforme a regra de negócio[cite: 5]
                    subscription.Status = SubscriptionStatus.Authorized;
                    subscription.LastModified = DateTime.UtcNow;

                    // Se o pagamento for aprovado hoje, podemos atualizar a data de modificação
                    await subscriptionRepository.UpdateAsync(subscription);

                    // Invalida o cache da assinatura para que o app/web perceba a liberação
                    await cacheService.RemoveAsync($"subscription:user:{localTransaction.UserId}");
                }
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // 5. Invalida o cache do status do pagamento[cite: 21]
            await cacheService.RemoveAsync($"payment_status_{localTransaction.Id}");

            logger.LogInformation("✅ Job concluído. Transação {TransactionId} e Assinatura processadas.",
                transactionId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "❌ Erro ao processar pagamento e liberar assinatura {PaymentId}.", resource.Id);
            throw; // Permite que o Hangfire realize o Retry[cite: 18]
        }
    }
}