using backend.Features.Subscription.Application.DTOs;
using backend.Features.Subscription.Application.Interfaces;
using backend.Features.Subscription.Domain.Entities;
using backend.Features.Subscription.Domain.Enums;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Persistence;

namespace backend.Features.Subscription.Application.Services;

public sealed class SubscriptionService(
    IMercadoPagoSubscriptionService mercadoPagoService, // Interface de comunicação injetada aqui
    ISubscriptionRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<SubscriptionService> logger
) : ISubscriptionService
{
    public async Task<SubscriptionResponse> CreateSubscriptionAsync(
        Guid userId,
        CreateSubscriptionRequest request
    )
    {
        var mpResult = await mercadoPagoService.CreateSubscriptionAsync(request).ConfigureAwait(false);

        await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            var newSubscription = new UserSubscription
            {
                UserId = userId,
                ExternalId = mpResult.Id,
                ExternalPlanId = request.PlanId,
                PayerEmail = request.PayerEmail,
                TransactionAmount = request.AutoRecurring.TransactionAmount,
                Status = Enum.Parse<SubscriptionStatus>(mpResult.Status, true),
                NextPaymentDate = mpResult.NextPaymentDate
            };

            await repository.CreateAsync(newSubscription).ConfigureAwait(false);
        }).ConfigureAwait(false);

        logger.LogInformation("Assinatura local criada com sucesso via MP. UserId: {UserId}", userId);
        return mpResult;
    }

    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(string externalId, UpdateSubscriptionRequest request)
    {
        var localSubscription = await repository.GetByExternalIdAsync(externalId).ConfigureAwait(false);
        if (localSubscription == null)
            throw new Exception($"Assinatura {externalId} não encontrada no banco local.");

        var mpResult = await mercadoPagoService.UpdateSubscriptionAsync(externalId, request).ConfigureAwait(false);

        await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            localSubscription.Status = Enum.Parse<SubscriptionStatus>(mpResult.Status, true);
            if (mpResult.NextPaymentDate.HasValue)
            {
                localSubscription.NextPaymentDate = mpResult.NextPaymentDate;
            }

            await repository.UpdateAsync(localSubscription).ConfigureAwait(false);
        }).ConfigureAwait(false);

        logger.LogInformation("Assinatura local {ExternalId} atualizada com sucesso.", externalId);
        return mpResult;
    }

    public async Task<SubscriptionResponse?> GetSubscriptionStatusAsync(Guid userId)
    {
        var localData = await repository.GetByUserIdAsync(userId).ConfigureAwait(false);

        if (localData == null) return null;

        return new SubscriptionResponse(
            Id: localData.ExternalId,
            Status: localData.Status.ToString().ToLower(), 
            Reason: "Plano Bueiro Inteligente",
            PayerId: 0,
            NextPaymentDate: localData.NextPaymentDate,
            DateCreated: localData.DateCreated,
            Summarized: null 
        );
    }
}