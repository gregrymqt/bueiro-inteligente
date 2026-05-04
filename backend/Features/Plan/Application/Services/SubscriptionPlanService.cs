using backend.Features.Plan.Application.DTOs;
using backend.Features.Plan.Application.Interfaces;
using backend.Features.Subscription.Application.Interfaces; // Interface do MP Plan Service
using backend.Features.Subscription.Domain.Entities;
using backend.Features.Subscription.Domain.Interfaces; // Repository
using Microsoft.Extensions.Logging;

namespace backend.Features.Plan.Application.Services;

public class SubscriptionPlanService(
    IMercadoPagoPlanService mercadoPagoPlanService,
    ISubscriptionPlanRepository planRepository,
    ILogger<SubscriptionPlanService> logger
) : ISubscriptionPlanService
{
    public async Task<PlanResponseDto> CreatePlanAsync(CreatePlanRequestDto request)
    {
        logger.LogInformation("Iniciando criação de plano: {PlanName}", request.Name);

        // 1. Monta o DTO exigido pelo Mercado Pago[cite: 25]
        var mpRequest = new MercadoPagoPlanRequest
        {
            Reason = request.Name,
            BackUrl = request.BackUrl,
            AutoRecurring = new AutoRecurringDTO
            {
                Frequency = request.Frequency,
                FrequencyType = request.FrequencyType,
                TransactionAmount = request.Amount,
                CurrencyId = "BRL"
            }
        };

        // 2. Chama a API do Mercado Pago[cite: 24]
        var mpResponse = await mercadoPagoPlanService.CreatePlanAsync(mpRequest);

        if (mpResponse == null || string.IsNullOrEmpty(mpResponse.Id))
        {
            throw new Exception("Falha ao criar o plano no gateway de pagamento Mercado Pago.");
        }

        // 3. Monta a entidade de Domínio para salvar localmente[cite: 26]
        var newPlan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            ExternalId = mpResponse.Id,
            Name = mpResponse.Reason,
            Amount = mpResponse.AutoRecurring.TransactionAmount,
            Frequency = mpResponse.AutoRecurring.Frequency,
            FrequencyType = mpResponse.AutoRecurring.FrequencyType,
            Status = mpResponse.Status,
            InitPoint = mpResponse.InitPoint
        };

        // 4. Persiste no PostgreSQL (já limpando o cache ativo)[cite: 27]
        await planRepository.CreateAsync(newPlan);

        return MapToResponseDto(newPlan);
    }

    public async Task<IEnumerable<PlanResponseDto>> GetAllActivePlansAsync()
    {
        // Busca do repositório, que implementa cache no Redis[cite: 27]
        var cacheResult = await planRepository.GetAllActiveAsync();

        return cacheResult.Data.Select(MapToResponseDto);
    }

    public async Task<PlanResponseDto> UpdatePlanAsync(Guid id, UpdatePlanRequestDto request)
    {
        // 1. Busca o plano localmente para obter o ExternalId
        var cacheResult = await planRepository.GetByIdAsync(id);
        var plan = cacheResult.Data;

        if (plan == null)
            throw new Exception($"Plano com ID {id} não encontrado.");

        logger.LogInformation("Atualizando plano {PlanName} ({ExternalId})", plan.Name, plan.ExternalId);

        // 2. Monta o request de atualização para o MP[cite: 25]
        var mpRequest = new MercadoPagoPlanRequest
        {
            Reason = request.Name,
            AutoRecurring = new AutoRecurringDTO
            {
                Frequency = plan.Frequency,
                FrequencyType = plan.FrequencyType,
                TransactionAmount = request.Amount,
                CurrencyId = "BRL"
            }
        };

        // 3. Atualiza na API do Mercado Pago[cite: 24]
        var mpResponse = await mercadoPagoPlanService.UpdatePlanAsync(plan.ExternalId, mpRequest);

        if (mpResponse == null)
            throw new Exception("Falha ao atualizar o plano no gateway de pagamento Mercado Pago.");

        // 4. Atualiza a entidade local e persiste no banco (já limpando os caches granulares)[cite: 26, 27]
        plan.Name = mpResponse.Reason;
        plan.Amount = mpResponse.AutoRecurring.TransactionAmount;
        plan.Status = mpResponse.Status;
        plan.InitPoint = mpResponse.InitPoint; // A InitPoint pode mudar ao atualizar

        await planRepository.UpdateAsync(plan);

        return MapToResponseDto(plan);
    }

    // Helper method para mapear a Entidade para DTO de Resposta
    private static PlanResponseDto MapToResponseDto(SubscriptionPlan plan)
    {
        return new PlanResponseDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Amount = plan.Amount,
            Status = plan.Status,
            InitPoint = plan.InitPoint
        };
    }
}