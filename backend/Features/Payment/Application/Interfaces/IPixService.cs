// Local: backend/Features/Payment/Application/Interfaces/IPixService.cs
using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface IPixService
{
    Task<PixPaymentResponseDto> CreatePixOrderAsync(CreatePixRequestDto request, Guid userId);
    Task<bool> RetryPixTransactionAsync(RetryPixRequestDto request, Guid userId);
}