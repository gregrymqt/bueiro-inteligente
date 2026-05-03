using backend.Features.Payment.Domain;

namespace backend.Features.Payment.Domain.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(PaymentTransaction transaction);
    Task UpdateAsync(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetByIdAsync(Guid id);
    Task<PaymentTransaction?> GetByPaymentIdAsync(long paymentId); // Novo: Busca por ID do Pagamento (Cartão/Pix)
    Task<PaymentTransaction?> GetByOrderIdAsync(string orderId); // Busca por ID da Ordem (Pix/Orders API)
}
