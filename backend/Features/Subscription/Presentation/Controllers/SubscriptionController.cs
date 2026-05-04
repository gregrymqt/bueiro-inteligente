using System.Security.Claims;
using backend.Features.Subscription.Application.DTOs;
using backend.Features.Subscription.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Subscription.Presentation.Controllers;

public sealed class SubscriptionController(
    ISubscriptionService subscriptionService,
    ILogger<SubscriptionController> logger
) : ApiControllerBase // Herda Authorize, RateLimit e prefixo de rota
{
    [HttpPost]
    public async Task<ActionResult<SubscriptionResponse>> Create([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            // Extrai o UserId do token JWT autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Usuário não identificado no token." });

            var result = await subscriptionService.CreateSubscriptionAsync(userId, request).ConfigureAwait(false);

            // Retorna 201 Created com os dados da assinatura do Mercado Pago[cite: 6, 10]
            return CreatedAtAction(nameof(GetStatus), result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar criação de assinatura.");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{externalId}")]
    public async Task<ActionResult<SubscriptionResponse>> Update(
        [FromRoute] string externalId,
        [FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            var result = await subscriptionService.UpdateSubscriptionAsync(externalId, request).ConfigureAwait(false);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar assinatura {ExternalId}.", externalId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<SubscriptionResponse>> GetStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var status = await subscriptionService.GetSubscriptionStatusAsync(userId).ConfigureAwait(false);

            if (status == null)
                return NotFound(new { message = "Nenhuma assinatura encontrada para este usuário." });

            return Ok(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar status da assinatura.");
            return StatusCode(500, new { message = "Erro interno ao recuperar dados." });
        }
    }
}