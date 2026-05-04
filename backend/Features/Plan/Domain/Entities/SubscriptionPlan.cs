namespace backend.Features.Subscription.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; set; }
    
    // ID retornado pelo Mercado Pago (ex: 2c938084...)
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // Mapeia para o 'reason' [cite: 1]
    public decimal Amount { get; set; } // Mapeia para 'transaction_amount' [cite: 1]
    
    public int Frequency { get; set; }
    public string FrequencyType { get; set; } = "months"; 
    
    public string Status { get; set; } = "active"; 
    public string InitPoint { get; set; } = string.Empty; 
    
    public DateTime DateCreated { get; set; }
    public DateTime LastModified { get; set; }
}