namespace backend.Features.Payment.Application.DTOs;

public record CreatePixRequestDto(
    decimal Amount,
    string Description,
    string PayerEmail,
    string FirstName,
    string LastName,
    string IdentificationType, // CPF ou CNPJ
    string IdentificationNumber,
    Guid? PlanId // Caso o Pix seja para assinar um plano específico
);

public record PixPaymentResponseDto(
    string OrderId,
    long PaymentId,
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
