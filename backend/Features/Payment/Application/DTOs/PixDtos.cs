namespace backend.Features.Payment.Application.DTOs;

public record CreatePixRequestDto(
    decimal Amount,
    string Description,
    string PayerEmail,
    string FirstName,
    string LastName,
    string? IdentificationType,   // 👈 Modificado para Nullable
    string? IdentificationNumber, // 👈 Modificado para Nullable
    Guid? PlanId 
);

public record PixPaymentResponseDto(
    string OrderId,
    string PaymentId,
    string Status,
    string StatusDetail,
    string QrCode, // Pix Copia e Cola
    string QrCodeBase64, // Imagem do QR Code
    string TicketUrl, // Link externo para instruções
    DateTimeOffset ExpirationDate,
    Guid ExternalReference // O ID da transação no nosso banco de dados
);

public record RetryPixRequestDto(
    string OrderId,
    string TransactionId // ID da transação que falhou (ex: PAY123...)
);
