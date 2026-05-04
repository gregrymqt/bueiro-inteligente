using backend.Features.Subscription.Application.DTOs;

namespace backend.Features.Subscription.Application.Interfaces;

public interface IMercadoPagoSubscriptionService
{
    Task<SubscriptionResponse> CreateSubscriptionAsync(CreateSubscriptionRequest request);
    Task<SubscriptionResponse> UpdateSubscriptionAsync(string externalId, UpdateSubscriptionRequest request);
    Task<SubscriptionResponse?> GetSubscriptionAsync(string externalId);
}