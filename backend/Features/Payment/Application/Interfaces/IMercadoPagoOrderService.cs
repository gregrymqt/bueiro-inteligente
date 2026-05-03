using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface IMercadoPagoOrderService
{
    Task<MpOrderResponse> CreateOrderAsync<TRequest>(TRequest request); // NOVO
    Task<MpOrderResponse?> GetOrderAsync(string? orderId);
    Task<bool> UpdateTransactionAsync(string orderId, string transactionId, MpUpdateTransactionRequest request);
    Task<bool> DeleteTransactionAsync(string orderId, string transactionId);
}