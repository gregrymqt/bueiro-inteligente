using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Notifications.Application;
using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore; // <-- ADICIONE ESTE USING IMPORTANTE

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessPaymentJob(
    ILogger<ProcessPaymentJob> logger,
    IMercadoPagoPaymentService mpPaymentService,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    AppDbContext dbContext,
    ICacheService cacheService,
    INotificationService notificationService
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation("🚀 Processando Pagamento MP: {PaymentId}", resource.Id);

        if (string.IsNullOrEmpty(resource.Id))
            return;

        var mpPaymentInfo =
            await mpPaymentService.GetPaymentAsync(resource.Id)
            ?? throw new Exception("Pagamento não encontrado na API do Mercado Pago.");

        if (!Guid.TryParse(mpPaymentInfo.ExternalReference, out Guid transactionId))
            return;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var localTransaction = await paymentRepository.GetByIdAsync(transactionId);
            if (localTransaction == null)
                return;

            if (localTransaction.Status == mpPaymentInfo.Status)
                return;

            long.TryParse(resource.Id, out long paymentIdLong);
            localTransaction.UpdateStatus(
                mpPaymentInfo.Status,
                mpPaymentInfo.StatusDetail,
                paymentIdLong
            );
            await paymentRepository.UpdateAsync(localTransaction);

            switch (mpPaymentInfo.Status)
            {
                case "approved":
                {
                    logger.LogInformation(
                        "✅ Pagamento aprovado. Ativando assinatura do usuário {UserId}...",
                        localTransaction.UserId
                    );

                    // 1. Busca o usuário com suas Roles atuais
                    var user = await dbContext
                        .Users.Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.Id == localTransaction.UserId);

                    if (user != null)
                    {
                        // 2. Verifica se ele já é Manager
                        bool isManager = user.Roles.Any(r => r.Name == "Manager");

                        if (!isManager)
                        {
                            // 3. Busca a Role Manager no banco (criada pelo Seed)
                            var managerRole = await dbContext.Roles.FirstOrDefaultAsync(r =>
                                r.Name == "Manager"
                            );

                            if (managerRole != null)
                            {
                                // 4. Adiciona a Role ao usuário
                                user.Roles.Add(managerRole);
                                logger.LogInformation(
                                    "👑 Role 'Manager' concedida ao usuário {UserId}.",
                                    user.Id
                                );
                            }
                        }
                    }
                    // ==========================================

                    await notificationService.SendNotificationAsync(
                        localTransaction.UserId,
                        "Pagamento Aprovado! 🎉",
                        $"Seu pagamento referente à transação {localTransaction.Id.ToString()[..8]} foi aprovado com sucesso. Você agora tem acesso de Manutenção!",
                        NotificationType.Success
                    );

                    var cacheSub = await subscriptionRepository.GetByUserIdAsync(
                        localTransaction.UserId
                    );
                    var subscription = cacheSub.Data;

                    if (subscription != null)
                    {
                        subscription.Status = SubscriptionStatus.Authorized;
                        subscription.LastModified = DateTime.UtcNow;

                        await subscriptionRepository.UpdateAsync(subscription);
                        await cacheService.RemoveAsync(
                            $"subscription:user:{localTransaction.UserId}"
                        );
                    }

                    break;
                }
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

            // O SaveChangesAsync irá persistir a nova Role na tabela de relacionamento UserRoles
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await cacheService.RemoveAsync($"payment_status_{localTransaction.Id}");

            logger.LogInformation(
                "✅ Job concluído. Transação {TransactionId} e Assinatura processadas.",
                transactionId
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(
                ex,
                "❌ Erro ao processar pagamento e liberar assinatura {PaymentId}.",
                resource.Id
            );
            throw;
        }
    }
}
