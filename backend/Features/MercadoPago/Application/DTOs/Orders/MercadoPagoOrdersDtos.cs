using System.Text.Json.Serialization;

namespace backend.Features.Payment.Application.DTOs
{
    // Request para /v1/orders
    public record MpOrderRequest(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("external_reference")] string ExternalReference,
        [property: JsonPropertyName("total_amount")] string TotalAmount,
        [property: JsonPropertyName("processing_mode")] string ProcessingMode,
        [property: JsonPropertyName("payer")] MpOrderPayer Payer,
        [property: JsonPropertyName("transactions")] MpOrderTransactions Transactions
    );

    public record MpOrderPayer([property: JsonPropertyName("email")] string Email);

    public record MpOrderTransactions(
        [property: JsonPropertyName("payments")] List<MpOrderPaymentRequest> Payments
    );

    public record MpOrderPaymentRequest(
        [property: JsonPropertyName("amount")] string Amount,
        [property: JsonPropertyName("payment_method")] MpOrderPaymentMethod PaymentMethod,
        [property: JsonPropertyName("expiration_time")] string? ExpirationTime = "P1D"
    );

    public record MpOrderPaymentMethod(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("type")] string Type
    );

    // Response simplificado de /v1/orders
    public class MpOrderResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDetail { get; set; } = string.Empty;
        public MpOrderResponseTransactions Transactions { get; set; } = new();
    }

    public class MpOrderResponseTransactions
    {
        public List<MpOrderResponsePayment> Payments { get; set; } = new();
    }

    public class MpOrderResponsePayment
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDetail { get; set; } = string.Empty;
        public MpOrderResponsePaymentMethod PaymentMethod { get; set; } = new();
        public DateTimeOffset? DateOfExpiration { get; set; }
    }

    public class MpOrderResponsePaymentMethod
    {
        public string QrCode { get; set; } = string.Empty;
        public string QrCodeBase64 { get; set; } = string.Empty;
        public string TicketUrl { get; set; } = string.Empty;
    }
}
