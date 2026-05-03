namespace backend.Features.Subscription.Domain.Enums;

public enum SubscriptionStatus
{
    // Assinatura sem método de pagamento
    Pending,
    
    // Assinatura com método de pagamento ativo
    Authorized,
    
    // Estados adicionais comuns para controle interno
    Paused,
    Cancelled
}