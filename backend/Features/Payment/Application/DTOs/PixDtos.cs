using System.Text.Json.Serialization;

namespace backend.Features.Payment.Application.DTOs;

public record CreatePixRequestDto(
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("payerEmail")] string PayerEmail,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("identificationType")] string? IdentificationType,
    [property: JsonPropertyName("identificationNumber")] string? IdentificationNumber,
    [property: JsonPropertyName("planId")] Guid? PlanId 
);

public record PixPaymentResponseDto(
    [property: JsonPropertyName("orderId")] string OrderId,
    [property: JsonPropertyName("paymentId")] string PaymentId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("statusDetail")] string StatusDetail,
    [property: JsonPropertyName("qrCode")] string QrCode,
    [property: JsonPropertyName("qrCodeBase64")] string QrCodeBase64,
    [property: JsonPropertyName("ticketUrl")] string TicketUrl,
    [property: JsonPropertyName("expirationDate")] DateTimeOffset ExpirationDate,
    [property: JsonPropertyName("externalReference")] Guid ExternalReference
);

public record RetryPixRequestDto(
    [property: JsonPropertyName("orderId")] string OrderId,
    [property: JsonPropertyName("transactionId")] string TransactionId
);