using backend.Features.Payment.Application.DTOs;

namespace backend.Features.Payment.Application.Interfaces;

public interface IMercadoPagoOrderService
{
    Task<MpOrderResponse?> GetOrderAsync(string orderId);
}
