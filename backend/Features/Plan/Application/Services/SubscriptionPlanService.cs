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
                CurrencyId = "BRL",
            },
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
            InitPoint = mpResponse.InitPoint,
            IsPopular = request.IsPopular,
            Features = request.Features, // Usa a propriedade NotMapped para serializar automaticamente
        };

        // 4. Persiste no PostgreSQL (já limpando o cache ativo)[cite: 27]
        await planRepository.CreateAsync(newPlan);

        return MapToResponseDto(newPlan);
    }

    public async Task<IEnumerable<PlanResponseDto>> GetAllActivePlansAsync()
    {
        var cacheResult = await planRepository.GetAllActiveAsync();
        return cacheResult.Data.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<PlanResponseDto>> GetAllPlansAsync()
    {
        var cacheResult = await planRepository.GetAllAsync();
        return cacheResult.Data.Select(MapToResponseDto);
    }

    public async Task<PlanResponseDto> GetPlanByIdAsync(Guid id)
    {
        // 1. Busca o plano pelo repositório (que já utiliza o CacheService)
        var cacheResult = await planRepository.GetByIdAsync(id);
        var plan = cacheResult.Data;

        // 2. Valida se o plano existe
        if (plan == null)
            throw new KeyNotFoundException($"Plano com ID {id} não encontrado.");

        // 3. Retorna mapeado para DTO
        return MapToResponseDto(plan);
    }

    public async Task<PlanResponseDto> UpdatePlanAsync(Guid id, UpdatePlanRequestDto request)
    {
        // 1. Busca o plano localmente para obter o ExternalId
        var cacheResult = await planRepository.GetByIdAsync(id);
        var plan = cacheResult.Data;

        if (plan == null)
            throw new Exception($"Plano com ID {id} não encontrado.");

        logger.LogInformation(
            "Atualizando plano {PlanName} ({ExternalId})",
            plan.Name,
            plan.ExternalId
        );

        // 2. Monta o request de atualização para o MP[cite: 25]
        var mpRequest = new MercadoPagoPlanRequest
        {
            Reason = request.Name,
            AutoRecurring = new AutoRecurringDTO
            {
                Frequency = plan.Frequency,
                FrequencyType = plan.FrequencyType,
                TransactionAmount = request.Amount,
                CurrencyId = "BRL",
            },
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
        plan.IsPopular = request.IsPopular;
        plan.Features = request.Features;

        await planRepository.UpdateAsync(plan);

        return MapToResponseDto(plan);
    }

    public async Task UpdatePlanStatusAsync(Guid id, string newStatus)
    {
        // Validação de segurança para garantir dados limpos
        var statusNormalizado = newStatus.ToLower().Trim();
        if (statusNormalizado != "active" && statusNormalizado != "inactive")
        {
            throw new ArgumentException("O status deve ser 'active' ou 'inactive'.");
        }

        var cacheResult = await planRepository.GetByIdAsync(id);
        var plan = cacheResult.Data;

        if (plan == null)
            throw new Exception("Plano não encontrado no sistema local.");

        // Atualiza apenas a propriedade localmente
        plan.Status = statusNormalizado;

        // Persiste a mudança no PostgreSQL e limpa o cache.
        // O Mercado Pago não é notificado dessa mudança de status.
        await planRepository.UpdateAsync(plan);

        logger.LogInformation(
            "Status do plano {PlanName} ({Id}) alterado para {Status} pelo Admin.",
            plan.Name,
            plan.Id,
            statusNormalizado
        );
    }

    // Helper method para mapear a Entidade para DTO de Resposta
    private static PlanResponseDto MapToResponseDto(SubscriptionPlan plan)
    {
        return new PlanResponseDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Price = plan.Amount, // Mapeia Amount (BD) para Price (Front-end)[cite: 28]
            Status = plan.Status,
            InitPoint = plan.InitPoint,
            Features = plan.Features,
            IsPopular = plan.IsPopular,
        };
    }
}
