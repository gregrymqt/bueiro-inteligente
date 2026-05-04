using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.Scheduler.Application.Interfaces;
using backend.Features.Subscription.Application.Interfaces;
using backend.Features.Subscription.Domain.Entities;
using backend.Features.Subscription.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Scheduler.Application.Jobs.MercadoPago;

public class ProcessSubscriptionPlanJob(
    IMercadoPagoPlanService mpPlanService,
    ISubscriptionPlanRepository planRepository,
    ILogger<ProcessSubscriptionPlanJob> logger
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData resource)
    {
        if (string.IsNullOrEmpty(resource.Id)) return;

        logger.LogInformation("🔄 Sincronizando Plano de Assinatura via Webhook: {Id}", resource.Id);

        try
        {
            // 1. Busca os dados atualizados diretamente na API do Mercado Pago[cite: 31]
            var mpPlan = await mpPlanService.GetPlanAsync(resource.Id);

            if (mpPlan == null)
            {
                logger.LogWarning("Plano {Id} não encontrado na API do Mercado Pago durante a sincronização.", resource.Id);
                return;
            }

            // 2. Tenta localizar o plano localmente pelo ExternalId
            var cacheResult = await planRepository.GetByExternalIdAsync(resource.Id);
            var localPlan = cacheResult.Data;

            if (localPlan == null)
            {
                // Cenário: Plano criado diretamente no painel do MP ou via outra integração
                logger.LogInformation("Criando plano inexistente localmente: {Id}", resource.Id);

                var newPlan = new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    ExternalId = mpPlan.Id,
                    Name = mpPlan.Reason,
                    Amount = mpPlan.AutoRecurring.TransactionAmount,
                    Frequency = mpPlan.AutoRecurring.Frequency,
                    FrequencyType = mpPlan.AutoRecurring.FrequencyType,
                    Status = mpPlan.Status,
                    InitPoint = mpPlan.InitPoint
                };

                await planRepository.CreateAsync(newPlan);
            }
            else
            {
                // Cenário: Atualização de nome, valor ou status do plano
                logger.LogInformation("Atualizando plano existente: {Id}", resource.Id);

                localPlan.Name = mpPlan.Reason;
                localPlan.Amount = mpPlan.AutoRecurring.TransactionAmount;
                localPlan.Status = mpPlan.Status;
                localPlan.InitPoint = mpPlan.InitPoint;
                localPlan.Frequency = mpPlan.AutoRecurring.Frequency;
                localPlan.FrequencyType = mpPlan.AutoRecurring.FrequencyType;

                await planRepository.UpdateAsync(localPlan);
            }

            logger.LogInformation("✅ Sincronização do plano {Id} concluída com sucesso.", resource.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar sincronização de plano para o ID: {Id}", resource.Id);
            throw; // Permite que o Hangfire realize o Retry automático em caso de falha temporária
        }
    }
}