using backend.Features.Subscription.Domain.Entities;

namespace backend.Features.Subscription.Domain.Interfaces;

public interface ISubscriptionRepository
{
    // Consultas
    Task<UserSubscription?> GetByIdAsync(Guid id);
    Task<UserSubscription?> GetByExternalIdAsync(string externalId);
    
    // Consulta direta para evitar leituras desatualizadas em fluxos de webhook
    Task<UserSubscription?> GetByUserIdAsync(Guid userId);
    
    // Comandos
    Task<UserSubscription> CreateAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
}