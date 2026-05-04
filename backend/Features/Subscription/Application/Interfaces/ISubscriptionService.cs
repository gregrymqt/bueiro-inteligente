using backend.Features.Subscription.Application.DTOs;

namespace backend.Features.Subscription.Application.Interfaces;

public interface ISubscriptionService
{
    // Cria a assinatura tanto no Mercado Pago quanto no banco de dados local
    Task<SubscriptionResponse> CreateSubscriptionAsync(Guid userId, CreateSubscriptionRequest request);

    // Atualiza a assinatura no Mercado Pago e sincroniza o banco
    Task<SubscriptionResponse> UpdateSubscriptionAsync(string externalId, UpdateSubscriptionRequest request);

    // Consulta o estado da assinatura, verificando o cache/banco
    Task<SubscriptionResponse?> GetSubscriptionStatusAsync(Guid userId);
}