// Local: backend/Features/Payment/Application/Interfaces/ICreditCardService.cs
using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface ICreditCardService
{
    Task<CreditCardPaymentResponseDto> CreateCreditCardOrderAsync(CreateCreditCardRequestDto request, Guid userId);
    Task<bool> RetryCreditCardTransactionAsync(RetryCreditCardRequestDto request, Guid userId);
}