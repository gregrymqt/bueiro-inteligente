using System;
using System.Text.Json.Serialization;

namespace backend.Features.Plan.Application.DTOs.Webhooks
{
    public class MercadoPagoWebhookNotification
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("data")]
        public MercadoPagoWebhookData? Data { get; set; }
    }

    public class MercadoPagoWebhookData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class ChargebackNotificationPayload
    {
        public string? Id { get; set; }
    }

    public class PaymentNotificationData
    {
        public string? Id { get; set; }
    }

    public class ClaimNotificationPayload
    {
        public string? Id { get; set; }
    }

    public class CardUpdateNotificationPayload
    {
        public string? NewCardId { get; set; }
        public string? CustomerId { get; set; }
    }
}
