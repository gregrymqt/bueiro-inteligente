using System.Text.Json.Serialization;

namespace backend.Features.Payment.Application.DTOs;

public record CreateCreditCardRequestDto(
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("payerEmail")] string PayerEmail,
    [property: JsonPropertyName("first_name")] string FirstName, // 🆕 Adicionado
    [property: JsonPropertyName("last_name")] string LastName,   // 🆕 Adicionado
    [property: JsonPropertyName("identificationType")] string? IdentificationType, // 🆕 Adicionado
    [property: JsonPropertyName("identificationNumber")] string? IdentificationNumber, // 🆕 Adicionado
    [property: JsonPropertyName("token")] string Token, 
    [property: JsonPropertyName("paymentMethodId")] string PaymentMethodId, 
    [property: JsonPropertyName("installments")] int Installments,
    [property: JsonPropertyName("planId")] Guid? PlanId
);

public record CreditCardPaymentResponseDto(
    [property: JsonPropertyName("orderId")] string OrderId,
    [property: JsonPropertyName("paymentId")] string PaymentId,
    [property: JsonPropertyName("status")] string Status, 
    [property: JsonPropertyName("statusDetail")] string StatusDetail, 
    [property: JsonPropertyName("externalResourceUrl")] string? ExternalResourceUrl, 
    [property: JsonPropertyName("externalReference")] Guid ExternalReference
);

public record RetryCreditCardRequestDto(
    [property: JsonPropertyName("orderId")] string OrderId,
    [property: JsonPropertyName("transactionId")] string TransactionId, 
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("paymentMethodId")] string PaymentMethodId,
    [property: JsonPropertyName("installments")] int Installments
);