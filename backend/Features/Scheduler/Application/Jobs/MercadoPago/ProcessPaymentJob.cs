// Local: backend/Features/Scheduler/Application/Jobs/MercadoPago/ProcessPaymentJob.cs

using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Application.Services;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Plan.Application.DTOs.Webhooks;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Infrastructure.Cache; // Ajuste para o seu ICacheService
using backend.Infrastructure.Persistence; // Ajuste para o seu DbContext
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessPaymentJob(
    ILogger<ProcessPaymentJob> logger,
    IMercadoPagoOrderService mpPaymentService, // Serviço que faz GET v1/payments/{id}
    IPaymentRepository paymentRepository,
    AppDbContext dbContext,
    ICacheService cacheService // Para invalidar o cache do Redis
// IPlanAccessService planAccessService -> Injete aqui o serviço que libera acesso ao bueiro/plano
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        logger.LogInformation(
            "🚀 Iniciando processamento do Pagamento MP: {PaymentId}",
            resource.Id
        );

        if (string.IsNullOrEmpty(resource.Id))
        {
            logger.LogWarning("Pagamento recebido sem ID. Ignorando.");
            return;
        }

        // 1. Consulta o status ATUALIZADO diretamente na API do Mercado Pago
        // Precisaremos deste endpoint (v1/payments/{id}) para pegar o status e o external_reference
        var mpPaymentInfo = await mpPaymentService.GetOrderAsync(resource.Id);

        if (mpPaymentInfo == null)
        {
            logger.LogWarning("Pagamento {PaymentId} não encontrado no Mercado Pago.", resource.Id);
            return;
        }

        // Converte o external_reference de volta para o nosso Guid
        if (!Guid.TryParse(mpPaymentInfo.ExternalReference, out Guid transactionId))
        {
            logger.LogWarning(
                "ExternalReference '{ExtRef}' inválido para o Pagamento {PaymentId}.",
                mpPaymentInfo.ExternalReference,
                resource.Id
            );
            return;
        }

        // 2. Inicia a Transação para garantir Atomicidade
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // 3. Idempotência: Busca a transação no banco
            var localTransaction = await paymentRepository.GetByIdAsync(transactionId);

            if (localTransaction == null)
            {
                logger.LogWarning("Transação local {TransactionId} não encontrada.", transactionId);
                return;
            }

            // Se o status no banco já for igual ao do MP, ignoramos (já foi processado)
            if (localTransaction.Status == mpPaymentInfo.Status)
            {
                logger.LogInformation(
                    "Transação {TransactionId} já está com status {Status}. Ignorando.",
                    transactionId,
                    localTransaction.Status
                );
                return;
            }

            // 4. Atualiza o status na entidade
            // O ID do pagamento do webhook vem como string, precisamos converter se for o caso
            long.TryParse(resource.Id, out long paymentIdLong);

            localTransaction.UpdateStatus(
                mpPaymentInfo.Status,
                mpPaymentInfo.StatusDetail,
                paymentIdLong
            );
            await paymentRepository.UpdateAsync(localTransaction);

            // 5. Regras de Negócio Pós-Pagamento (Ex: Liberar acesso ao Painel do Bueiro)
            if (mpPaymentInfo.Status == "approved")
            {
                logger.LogInformation(
                    "✅ Pagamento {PaymentId} APROVADO. Liberando acesso para o usuário {UserId}.",
                    resource.Id,
                    localTransaction.UserId
                );

                // Exemplo: await planAccessService.GrantAccessAsync(localTransaction.UserId, localTransaction.PlanId);
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // 6. Invalidação de Cache (Redis)
            // Para que o frontend (React) perceba a mudança imediatamente
            await cacheService.RemoveAsync($"payment_status_{localTransaction.Id}");

            logger.LogInformation(
                "✅ Job concluído e transação {TransactionId} atualizada com sucesso.",
                transactionId
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "❌ Erro crítico ao processar pagamento {PaymentId}.", resource.Id);

            // O Hangfire captura essa exceção e agenda um retry automático com base no delay configurado[cite: 15].
            throw;
        }
    }
}
