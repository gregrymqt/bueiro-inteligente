namespace backend.Features.Payment.Application.DTOs;

public record CreateCreditCardRequestDto(
    decimal Amount,
    string Description,
    string PayerEmail,
    string Token, // Gerado pelo frontend
    string PaymentMethodId, // ex: 'visa', 'master'
    int Installments,
    Guid? PlanId
);

public record CreditCardPaymentResponseDto(
    string OrderId,
    long PaymentId,
    string Status, // ex: 'processed'
    string StatusDetail, // ex: 'accredited'
    string? ExternalResourceUrl, // URL para validações extras (3DS/Remedies)
    Guid ExternalReference
);

public record RetryCreditCardRequestDto(
    string OrderId,
    string TransactionId, // ID da transação que falhou (ex: PAY123...)
    string Token,
    string PaymentMethodId,
    int Installments
);