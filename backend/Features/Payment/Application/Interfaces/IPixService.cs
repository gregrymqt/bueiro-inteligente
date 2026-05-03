using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface IPixService
{
    /// <summary>
    /// Cria uma ordem de pagamento via Pix e retorna os dados para renderização do QR Code.
    /// </summary>
    Task<PixPaymentResponseDto> CreatePixOrderAsync(CreatePixRequestDto request, Guid userId);
}
