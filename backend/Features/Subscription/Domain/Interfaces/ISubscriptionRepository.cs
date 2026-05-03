using backend.Features.Subscription.Domain.Entities;
using backend.Infrastructure.Cache;

namespace backend.Features.Subscription.Domain.Interfaces;

public interface ISubscriptionRepository
{
    // Consultas
    Task<UserSubscription?> GetByIdAsync(Guid id);
    Task<UserSubscription?> GetByExternalIdAsync(string externalId);
    
    // Consulta otimizada com Cache para uso constante em validações de acesso
    Task<CacheResponseDto<UserSubscription?>> GetByUserIdAsync(Guid userId);
    
    // Comandos
    Task<UserSubscription> CreateAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
}