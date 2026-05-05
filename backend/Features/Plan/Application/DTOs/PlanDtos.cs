using System.Text.Json.Serialization;

namespace backend.Features.Plan.Application.DTOs;

public class MercadoPagoPlanRequest
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("auto_recurring")]
    public AutoRecurringDTO AutoRecurring { get; set; } = new();

    [JsonPropertyName("back_url")]
    public string? BackUrl { get; set; }
}

public class MercadoPagoPlanResponse : MercadoPagoPlanRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("init_point")]
    public string InitPoint { get; set; } = string.Empty;

    [JsonPropertyName("external_reference")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("date_created")]
    public DateTime DateCreated { get; set; }
}

public class AutoRecurringDTO
{
    [JsonPropertyName("frequency")]
    public int Frequency { get; set; }

    [JsonPropertyName("frequency_type")]
    public string FrequencyType { get; set; } = "months";

    [JsonPropertyName("transaction_amount")]
    public decimal TransactionAmount { get; set; }

    [JsonPropertyName("currency_id")]
    public string CurrencyId { get; set; } = "BRL";

    [JsonPropertyName("billing_day")]
    public int? BillingDay { get; set; }

    [JsonPropertyName("free_trial")]
    public FreeTrialDTO? FreeTrial { get; set; }
}

public class FreeTrialDTO
{
    [JsonPropertyName("frequency")]
    public int Frequency { get; set; }

    [JsonPropertyName("frequency_type")]
    public string FrequencyType { get; set; } = "months";
}

//-------------------------------------------------

public class CreatePlanRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Frequency { get; set; } = 1;
    public string FrequencyType { get; set; } = "months";
    public string? BackUrl { get; set; }

    // Novos campos que o Admin enviará via Front-end
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; } = false;
}

public class UpdatePlanRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Admin pode mudar as features ou a popularidade
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; } = false;
}

public class UpdatePlanStatusRequestDto
{
    // Status esperado: "active" ou "inactive"
    public string Status { get; set; } = string.Empty;
}

public class PlanResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } // Ajustado para bater com 'price' no TS
    public string Status { get; set; } = string.Empty;
    public string InitPoint { get; set; } = string.Empty;

    // Campos visuais
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; }
}