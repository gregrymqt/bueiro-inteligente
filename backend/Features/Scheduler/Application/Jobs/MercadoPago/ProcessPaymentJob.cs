using backend.Features.Plan.Application.DTOs.Webhooks;
using backend.Features.Scheduler.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessPaymentJob(
    ILogger<ProcessPaymentJob> logger
/* , IPaymentService paymentService */
// Injetar o service da feature Payment aqui futuramente
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation(
            "🚀 Iniciando processamento do Pagamento MP: {PaymentId}",
            resource.Id
        );

        try
        {
            // 1. Chamar o Service da Feature Payment/Plan
            // 2. O Service usará o MercadoPagoServiceBase para buscar os detalhes do pagamento via GET /v1/payments/{id}
            // 3. O Service atualizará o status no banco de dados (ex: Pago, Pendente, Cancelado)

            await Task.Delay(500); // Simulação de processamento

            logger.LogInformation("✅ Pagamento {PaymentId} processado com sucesso.", resource.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro crítico ao processar pagamento {PaymentId}.", resource.Id);
            throw; // Re-throw para o Hangfire realizar o retry automático
        }
    }
}
