using backend.Features.Auth.Domain.Interfaces;
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

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessPaymentJob(
    ILogger<ProcessPaymentJob> logger,
    IMercadoPagoPaymentService mpPaymentService,
    IAuthRepository authRepository,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
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

        Guid? notificationUserId = null;
        string? notificationTitle = null;
        string? notificationMessage = null;
        NotificationType? notificationType = null;

        await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            var localTransaction = await paymentRepository.GetByIdAsync(transactionId);
            if (localTransaction == null)
                return;

            if (localTransaction.Status == mpPaymentInfo.Status)
                return;

            localTransaction.UpdateStatus(
                mpPaymentInfo.Status,
                mpPaymentInfo.StatusDetail,
                resource.Id
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

                    var user = await authRepository.GetUserByIdAsync(localTransaction.UserId);

                    if (user != null)
                    {
                        bool isManager = user.Roles.Any(r => r.Name == "Manager");

                        if (!isManager)
                        {
                            var managerRole = await authRepository.GetRoleByNameAsync("Manager");

                            if (managerRole != null)
                            {
                                user.Roles.Add(managerRole);
                                logger.LogInformation(
                                    "👑 Role 'Manager' concedida ao usuário {UserId}.",
                                    user.Id
                                );
                            }
                        }
                    }

                    notificationUserId = localTransaction.UserId;
                    notificationTitle = "Pagamento Aprovado! 🎉";
                    notificationMessage =
                        $"Seu pagamento referente à transação {localTransaction.Id.ToString()[..8]} foi aprovado com sucesso. Você agora tem acesso de Manutenção!";
                    notificationType = NotificationType.Success;

                    var subscription = await subscriptionRepository.GetByUserIdAsync(
                        localTransaction.UserId
                    );

                    if (subscription != null)
                    {
                        subscription.Status = SubscriptionStatus.Authorized;
                        subscription.LastModified = DateTime.UtcNow;

                        await subscriptionRepository.UpdateAsync(subscription);
                    }

                    break;
                }
                case "rejected":
                case "cancelled":
                    notificationUserId = localTransaction.UserId;
                    notificationTitle = "Pagamento Recusado";
                    notificationMessage =
                        "Houve um problema ao processar seu pagamento. Verifique a transação.";
                    notificationType = NotificationType.Error;
                    break;
            }

            logger.LogInformation(
                "✅ Job concluído. Transação {TransactionId} e Assinatura processadas.",
                transactionId
            );
        });

        if (
            notificationUserId.HasValue
            && notificationTitle is not null
            && notificationMessage is not null
            && notificationType.HasValue
        )
        {
            await notificationService.SendNotificationAsync(
                notificationUserId.Value,
                notificationTitle,
                notificationMessage,
                notificationType.Value
            );
        }

        await cacheService.RemoveAsync($"payment_status_{transactionId}");
    }
}
