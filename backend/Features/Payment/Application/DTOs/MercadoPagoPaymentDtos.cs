
using System.Text.Json.Serialization;

namespace backend.Features.Payment.Application.DTOs
{
    public class MpPaymentResponse
    {
        // O ID de pagamento no MP é um número longo[cite: 16]
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("status_detail")]
        public string StatusDetail { get; set; } = string.Empty;

        // Propriedade vital: mapeia para o Guid da nossa tabela PaymentTransaction[cite: 16]
        [JsonPropertyName("external_reference")]
        public string ExternalReference { get; set; } = string.Empty;

        [JsonPropertyName("payment_method_id")]
        public string PaymentMethodId { get; set; } = string.Empty;

        [JsonPropertyName("payment_type_id")]
        public string PaymentTypeId { get; set; } = string.Empty;

        [JsonPropertyName("date_approved")]
        public DateTimeOffset? DateApproved { get; set; }
    }
}
