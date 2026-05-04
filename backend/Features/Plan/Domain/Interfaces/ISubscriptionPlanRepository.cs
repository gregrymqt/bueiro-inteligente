using backend.Features.Subscription.Domain.Entities;
using backend.Infrastructure.Cache;

namespace backend.Features.Subscription.Domain.Interfaces;

public interface ISubscriptionPlanRepository
{
    /// <summary>
    /// Busca um plano de assinatura pelo seu ID interno.
    /// Utiliza o Redis para otimização de leitura.
    /// </summary>
    Task<CacheResponseDto<SubscriptionPlan?>> GetByIdAsync(Guid id);

    /// <summary>
    /// Busca um plano de assinatura pelo seu ID no Mercado Pago (ExternalId).
    /// Utiliza o Redis para otimização de leitura.
    /// </summary>
    Task<CacheResponseDto<SubscriptionPlan?>> GetByExternalIdAsync(string externalId);

    /// <summary>
    /// Lista todos os planos de assinatura ativos.
    /// Utiliza o Redis para otimização de leitura.
    /// </summary>
    Task<CacheResponseDto<IEnumerable<SubscriptionPlan>>> GetAllActiveAsync();

    Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
}