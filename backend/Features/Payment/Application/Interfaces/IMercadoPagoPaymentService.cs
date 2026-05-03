namespace backend.Features.Payment.Application.Interfaces;

using DTOs;

public interface IMercadoPagoPaymentService
{
    Task<MpPaymentResponse?> GetPaymentAsync(string paymentId);
}
