using System.Security.Claims;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Payment.Presentation.Controllers;

public class CreditCardController(ICreditCardService creditCardService) : ApiControllerBase
{
    [HttpPost("process")]
    public async Task<ActionResult<CreditCardPaymentResponseDto>> ProcessPayment(
        [FromBody] CreateCreditCardRequestDto request
    )
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Usuário não identificado.");
        }

        var response = await creditCardService.CreateCreditCardOrderAsync(request, userId);

        return Ok(response);
    }

    [HttpPut("retry")]
    public async Task<ActionResult> RetryPayment([FromBody] RetryCreditCardRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var success = await creditCardService.RetryCreditCardTransactionAsync(request, userId);

        if (success)
        {
            return Accepted(new { message = "Retentativa enviada com sucesso. O status será atualizado via Webhook." });
        }

        return BadRequest(new { error = "Falha ao processar retentativa com o Mercado Pago." });
    }
}