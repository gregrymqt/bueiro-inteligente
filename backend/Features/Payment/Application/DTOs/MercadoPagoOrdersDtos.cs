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

    public class MpOrderResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("status_detail")]
        public string StatusDetail { get; set; } = string.Empty;

        // Propriedade vital para nossa idempotência no Webhook (ID do nosso banco)
        [JsonPropertyName("external_reference")]
        public string ExternalReference { get; set; } = string.Empty;

        [JsonPropertyName("transactions")]
        public MpOrderResponseTransactions Transactions { get; set; } = new();
    }

    public class MpOrderResponseTransactions
    {
        [JsonPropertyName("payments")]
        public List<MpOrderResponsePayment> Payments { get; set; } = new();
    }

    public class MpOrderResponsePayment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("status_detail")]
        public string StatusDetail { get; set; } = string.Empty;

        [JsonPropertyName("payment_method")]
        public MpOrderResponsePaymentMethod PaymentMethod { get; set; } = new();

        [JsonPropertyName("date_of_expiration")]
        public DateTimeOffset? DateOfExpiration { get; set; }
    }

    public class MpOrderResponsePaymentMethod
    {
        [JsonPropertyName("qr_code")]
        public string QrCode { get; set; } = string.Empty;

        [JsonPropertyName("qr_code_base64")]
        public string QrCodeBase64 { get; set; } = string.Empty;

        [JsonPropertyName("ticket_url")]
        public string TicketUrl { get; set; } = string.Empty;
    }
}
