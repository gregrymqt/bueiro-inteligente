namespace backend.Features.Payment.Application.Interfaces;

using backend.Features.Payment.Application.DTOs;

public interface IMercadoPagoPaymentService
{
    Task<MpPaymentResponse?> GetPaymentAsync(string paymentId);
}
