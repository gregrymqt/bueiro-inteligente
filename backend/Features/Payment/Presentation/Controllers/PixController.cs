using System.Security.Claims;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Payment.Presentation.Controllers;

public class PixController(IPixService pixService) : ApiControllerBase
{
    [HttpPost("create-order")]
    public async Task<ActionResult<PixPaymentResponseDto>> CreateOrder(
        [FromBody] CreatePixRequestDto request
    )
    {
        // Extrai o UserId do Token JWT (injetado pelo [Authorize] na classe base)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Usuário não identificado no token.");
        }
        // O IPixService será responsável por:
        // 1. Persistir a transação pendente no nosso banco.
        // 2. Chamar o endpoint /v1/orders do Mercado Pago com X-Idempotency-Key.
        // 3. Retornar os dados do QR Code para o Frontend.
        var response = await pixService.CreatePixOrderAsync(request, userId);

        return Ok(response);
    }
}
