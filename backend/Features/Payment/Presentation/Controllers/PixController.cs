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
        
        var response = await pixService.CreatePixOrderAsync(request, userId);

        return Ok(response);
    }

    [HttpPut("retry")]
    public async Task<ActionResult> RetryOrder([FromBody] RetryPixRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var success = await pixService.RetryPixTransactionAsync(request, userId);

        if (success)
        {
            return Accepted(new { message = "Retentativa de Pix enviada com sucesso. Aguardando processamento." });
        }

        return BadRequest(new { error = "Falha ao processar retentativa de Pix." });
    }
}