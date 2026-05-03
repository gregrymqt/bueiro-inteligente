using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface IPreferenceService
{
    /// <summary>
    /// Cria uma preferência no Mercado Pago e retorna o link para o Checkout Pro.
    /// </summary>
    Task<PreferenceResponseDto> CreatePreferenceAsync(CreatePreferenceRequestDto request, Guid userId);
}