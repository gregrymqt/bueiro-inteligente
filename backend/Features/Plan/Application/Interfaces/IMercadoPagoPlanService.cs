using backend.Features.Plan.Application.DTOs;

namespace backend.Features.Subscription.Application.Interfaces;

public interface IMercadoPagoPlanService
{
    /// <summary>
    /// Cria um novo plano de assinatura no Mercado Pago (POST).
    /// </summary>
    Task<MercadoPagoPlanResponse?> CreatePlanAsync(MercadoPagoPlanRequest request);

    /// <summary>
    /// Busca os detalhes de um plano de assinatura existente (GET).
    /// </summary>
    Task<MercadoPagoPlanResponse?> GetPlanAsync(string planId);

    /// <summary>
    /// Atualiza um plano de assinatura existente (PUT).
    /// </summary>
    Task<MercadoPagoPlanResponse?> UpdatePlanAsync(string planId, MercadoPagoPlanRequest request);
}