using backend.Features.Plan.Application.DTOs;

namespace backend.Features.Plan.Application.Interfaces;

public interface ISubscriptionPlanService
{
    /// <summary>
    /// Cria um plano no Mercado Pago e o persiste no banco local.
    /// </summary>
    Task<PlanResponseDto> CreatePlanAsync(CreatePlanRequestDto request);

    /// <summary>
    /// Lista todos os planos ativos a partir do cache/banco.
    /// </summary>
    Task<IEnumerable<PlanResponseDto>> GetAllActivePlansAsync();

    /// <summary>
    /// Atualiza o plano no Mercado Pago e reflete a mudança no banco local.
    /// </summary>
    Task<PlanResponseDto> UpdatePlanAsync(Guid id, UpdatePlanRequestDto request);

    Task UpdatePlanStatusAsync(Guid id, string newStatus);

    Task<IEnumerable<PlanResponseDto>> GetAllPlansAsync();
}