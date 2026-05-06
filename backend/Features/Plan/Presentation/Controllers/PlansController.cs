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
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PlanResponseDto>>> GetActivePlans()
    {
        var plans = await planService.GetAllActivePlansAsync();
        return Ok(plans);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PlanResponseDto>>> GetAllPlans()
    {
        var plans = await planService.GetAllPlansAsync();
        return Ok(plans);
    }

    /// <summary>
    /// Busca os detalhes de um plano específico pelo seu ID.
    /// Utilizado para renderizar valores na tela de checkout.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PlanResponseDto>> GetPlanById(Guid id)
    {
        try
        {
            var plan = await planService.GetPlanByIdAsync(id);
            return Ok(plan);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Tentativa de acessar plano inexistente: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TogglePlanStatus(
        Guid id,
        [FromBody] UpdatePlanStatusRequestDto request
    )
    {
        await planService.UpdatePlanStatusAsync(id, request.Status);
        return NoContent();
    }

    /// <summary>
    /// Cria um novo plano de assinatura integrado ao Mercado Pago.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Apenas administradores devem criar planos
    public async Task<ActionResult<PlanResponseDto>> CreatePlan(
        [FromBody] CreatePlanRequestDto request
    )
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
    public async Task<ActionResult<PlanResponseDto>> UpdatePlan(
        Guid id,
        [FromBody] UpdatePlanRequestDto request
    )
    {
        logger.LogInformation("Recebida requisição para atualizar plano: {Id}", id);

        var response = await planService.UpdatePlanAsync(id, request);

        return Ok(response);
    }
}
