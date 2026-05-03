using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface ICreditCardService
{
    /// <summary>
    /// Processa um pagamento via Cartão de Crédito utilizando a API de Orders.
    /// </summary>
    Task<CreditCardPaymentResponseDto> CreateCreditCardOrderAsync(
        CreateCreditCardRequestDto request,
        Guid userId
    );
}
