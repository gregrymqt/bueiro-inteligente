using backend.Features.Plan.Application.DTOs;
using backend.Features.Plan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Plan.Presentation.Controllers;

public class PlansController(ISubscriptionPlanService planService, ILogger<PlansController> logger) 
    : ApiControllerBase
{
    /// <summary>
    /// Lista todos os planos de assinatura ativos.
    /// Ideal para renderizar a tela de pricing no frontend.
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Geralmente planos são públicos para visualização
    public async Task<ActionResult<IEnumerable<PlanResponseDto>>> GetActivePlans()
    {
        var plans = await planService.GetAllActivePlansAsync();
        return Ok(plans);
    }

    /// <summary>
    /// Cria um novo plano de assinatura integrado ao Mercado Pago.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Apenas administradores devem criar planos
    public async Task<ActionResult<PlanResponseDto>> CreatePlan([FromBody] CreatePlanRequestDto request)
    {
        logger.LogInformation("Recebida requisição para criar plano: {Name}", request.Name);
        
        var response = await planService.CreatePlanAsync(request);
        
        return CreatedAtAction(nameof(GetActivePlans), new { id = response.Id }, response);
    }

    /// <summary>
    /// Atualiza nome ou valor de um plano existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PlanResponseDto>> UpdatePlan(Guid id, [FromBody] UpdatePlanRequestDto request)
    {
        logger.LogInformation("Recebida requisição para atualizar plano: {Id}", id);
        
        var response = await planService.UpdatePlanAsync(id, request);
        
        return Ok(response);
    }
}