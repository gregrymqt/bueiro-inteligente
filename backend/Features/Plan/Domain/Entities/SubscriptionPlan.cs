// backend.Features.Subscription.Domain.Entities.SubscriptionPlan.cs
using System.Text.Json;

namespace backend.Features.Subscription.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public int Frequency { get; set; }
    public string FrequencyType { get; set; } = "months";

    public string Status { get; set; } = "active";
    public string InitPoint { get; set; } = string.Empty;

    // --- Novos campos para suportar o Front-end ---
    public bool IsPopular { get; set; } = false;

    // O EF Core salvará isso como texto (JSON) no banco de dados.
    public string FeaturesJson { get; set; } = "[]";

    // Propriedade não-mapeada (Ignorada pelo EF) apenas para facilitar o uso no código
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<string> Features
    {
        get => string.IsNullOrEmpty(FeaturesJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(FeaturesJson)!;
        set => FeaturesJson = JsonSerializer.Serialize(value);
    }
    // ----------------------------------------------

    public DateTime DateCreated { get; set; }
    public DateTime LastModified { get; set; }
}