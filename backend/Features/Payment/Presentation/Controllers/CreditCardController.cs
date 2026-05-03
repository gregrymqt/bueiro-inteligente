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
        // Recupera o ID do usuário logado via Claims do JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Usuário não identificado.");
        }

        try
        {
            // O serviço orquestrará a transação no banco e a chamada à API Orders
            var response = await creditCardService.CreateCreditCardOrderAsync(request, userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            // O tratamento detalhado de erros e rollback ocorre dentro da Service
            throw;
        }
    }
}
