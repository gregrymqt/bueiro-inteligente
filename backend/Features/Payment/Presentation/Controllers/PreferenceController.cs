using System.Security.Claims;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Payment.Presentation.Controllers;

public class PreferenceController(IPreferenceService preferenceService) : ApiControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<PreferenceResponseDto>> CreatePreference(
        [FromBody] CreatePreferenceRequestDto request
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
            // O serviço criará a transação no banco e a preferência no SDK
            var response = await preferenceService.CreatePreferenceAsync(request, userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            // O tratamento de erro e rollback ocorrerá na implementação da Service
            throw;
        }
    }
}
