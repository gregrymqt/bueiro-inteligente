using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Scheduler.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPaymentJob(ILogger<ProcessSubscriptionPaymentJob> logger)
    : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation(
            "💳 Processando renovação/pagamento de Assinatura: {Id}",
            resource.Id
        );

        // Lógica: Validar se a recorrência foi aprovada e liberar o acesso do usuário no sistema
        await Task.CompletedTask;
    }
}
