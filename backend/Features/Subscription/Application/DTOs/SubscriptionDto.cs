using System.Text.Json.Serialization;

namespace backend.Features.Subscription.Application.DTOs;

//Objeto de Recorrência (Comum)
public record AutoRecurringDto(
    [property: JsonPropertyName("frequency")] int Frequency,
    [property: JsonPropertyName("frequency_type")] string FrequencyType, // months, days
    [property: JsonPropertyName("transaction_amount")] decimal TransactionAmount,
    [property: JsonPropertyName("currency_id")] string CurrencyId = "BRL",
    [property: JsonPropertyName("start_date")] DateTime? StartDate = null,
    [property: JsonPropertyName("end_date")] DateTime? EndDate = null
);

//Criar Assinatura (Request)
public record CreateSubscriptionRequest(
    [property: JsonPropertyName("preapproval_plan_id")] string PlanId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("external_reference")] string ExternalReference,
    [property: JsonPropertyName("payer_email")] string PayerEmail,
    [property: JsonPropertyName("card_token_id")] string CardTokenId,
    [property: JsonPropertyName("auto_recurring")] AutoRecurringDto AutoRecurring,
    [property: JsonPropertyName("back_url")] string BackUrl,
    [property: JsonPropertyName("status")] string Status = "authorized"
);

//Atualizar Assinatura (Update Request)
public record UpdateSubscriptionRequest(
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("status")] string? Status, // "pending", "authorized", "cancelled"
    [property: JsonPropertyName("card_token_id")] string? CardTokenId,
    [property: JsonPropertyName("auto_recurring")] AutoRecurringUpdateDto? AutoRecurring
);

public record AutoRecurringUpdateDto(
    [property: JsonPropertyName("transaction_amount")] decimal TransactionAmount
);

//Resposta da API (Subscription Response)
public record SubscriptionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("payer_id")] long PayerId,
    [property: JsonPropertyName("next_payment_date")] DateTime? NextPaymentDate,
    [property: JsonPropertyName("date_created")] DateTime DateCreated,
    [property: JsonPropertyName("summarized")] SubscriptionSummaryDto? Summarized
);

public record SubscriptionSummaryDto(
    [property: JsonPropertyName("charged_amount")] decimal ChargedAmount,
    [property: JsonPropertyName("pending_charge_amount")] decimal PendingAmount,
    [property: JsonPropertyName("last_charged_date")] DateTime? LastChargedDate
);