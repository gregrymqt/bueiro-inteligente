using backend.Features.Subscription.Domain.Enums;

namespace backend.Features.Subscription.Domain.Entities;

public class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // ID da assinatura no Mercado Pago (ex: 2c938084...)
    public string ExternalId { get; set; } = string.Empty;
    public string? ExternalPlanId { get; set; }
    
    public string PayerEmail { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public decimal TransactionAmount { get; set; }
    
    public DateTime? NextPaymentDate { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastModified { get; set; }
}