using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Scheduler.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPreapprovalJob(ILogger<ProcessSubscriptionPreapprovalJob> logger)
    : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation(
            "📝 Processando registro/vínculo de Assinatura (Preapproval): {Id}",
            resource.Id
        );

        // Lógica: Vincular o ID da assinatura do MP ao perfil do usuário no Bueiro Inteligente
        await Task.CompletedTask;
    }
}
