using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;
using backend.Features.Notifications.Application;
using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces; // <-- ADICIONADO NOVO NAMESPACE

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessPaymentJob(
    ILogger<ProcessPaymentJob> logger,
    IMercadoPagoPaymentService mpPaymentService,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    AppDbContext dbContext,
    ICacheService cacheService,
    INotificationService notificationService // <-- INJEÇÃO DO NOVO SERVIÇO (Substituiu IRealtimeService)
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation("🚀 Processando Pagamento MP: {PaymentId}", resource.Id);

        if (string.IsNullOrEmpty(resource.Id)) return;

        var mpPaymentInfo = await mpPaymentService.GetPaymentAsync(resource.Id) ??
                            throw new Exception("Pagamento não encontrado na API do Mercado Pago.");

        if (!Guid.TryParse(mpPaymentInfo.ExternalReference, out Guid transactionId)) return;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var localTransaction = await paymentRepository.GetByIdAsync(transactionId);
            if (localTransaction == null) return;

            if (localTransaction.Status == mpPaymentInfo.Status) return;

            long.TryParse(resource.Id, out long paymentIdLong);
            localTransaction.UpdateStatus(mpPaymentInfo.Status, mpPaymentInfo.StatusDetail, paymentIdLong);
            await paymentRepository.UpdateAsync(localTransaction);

            switch (mpPaymentInfo.Status)
            {
                case "approved":
                {
                    logger.LogInformation("✅ Pagamento aprovado. Ativando assinatura do usuário {UserId}...",
                        localTransaction.UserId);

                    // Agora o evento fica salvo no banco e dispara no WebSocket automaticamente
                    await notificationService.SendNotificationAsync(
                        localTransaction.UserId,
                        "Pagamento Aprovado! 🎉",
                        $"Seu pagamento referente à transação {localTransaction.Id.ToString()[..8]} foi aprovado com sucesso.",
                        NotificationType.Success
                    );

                    var cacheSub = await subscriptionRepository.GetByUserIdAsync(localTransaction.UserId);
                    var subscription = cacheSub.Data;

                    if (subscription != null)
                    {
                        subscription.Status = SubscriptionStatus.Authorized;
                        subscription.LastModified = DateTime.UtcNow;

                        await subscriptionRepository.UpdateAsync(subscription);
                        await cacheService.RemoveAsync($"subscription:user:{localTransaction.UserId}");
                    }

                    break;
                }
                // VOCÊ PODE ADICIONAR UM ELSE AQUI PARA AVISAR QUANDO FALHAR:
                case "rejected":
                case "cancelled":
                    await notificationService.SendNotificationAsync(
                        localTransaction.UserId,
                        "Pagamento Recusado",
                        "Houve um problema ao processar seu pagamento. Verifique a transação.",
                        NotificationType.Error
                    );
                    break;
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await cacheService.RemoveAsync($"payment_status_{localTransaction.Id}");

            logger.LogInformation("✅ Job concluído. Transação {TransactionId} e Assinatura processadas.",
                transactionId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "❌ Erro ao processar pagamento e liberar assinatura {PaymentId}.", resource.Id);
            throw;
        }
    }
}