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
    AppDbContext dbContext,
    ILogger<SubscriptionService> logger
) : ISubscriptionService
{
    public async Task<SubscriptionResponse> CreateSubscriptionAsync(Guid userId, CreateSubscriptionRequest request)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            // 1. Chama o Mercado Pago usando a interface separada
            var mpResult = await mercadoPagoService.CreateSubscriptionAsync(request).ConfigureAwait(false);
            
            // 2. Monta a entidade para o banco de dados
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

            // 3. Salva no banco e limpa o cache via repositório
            await repository.CreateAsync(newSubscription).ConfigureAwait(false);

            await transaction.CommitAsync().ConfigureAwait(false);
            
            logger.LogInformation("Assinatura local criada com sucesso via MP. UserId: {UserId}", userId);
            return mpResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            logger.LogError(ex, "Rollback executado ao criar assinatura para UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(string externalId, UpdateSubscriptionRequest request)
    {
        var localSubscription = await repository.GetByExternalIdAsync(externalId).ConfigureAwait(false);
        if (localSubscription == null)
            throw new Exception($"Assinatura {externalId} não encontrada no banco local.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            // 1. Atualiza no Mercado Pago usando o serviço separado
            var mpResult = await mercadoPagoService.UpdateSubscriptionAsync(externalId, request).ConfigureAwait(false);

            // 2. Atualiza a entidade local
            localSubscription.Status = Enum.Parse<SubscriptionStatus>(mpResult.Status, true);
            if (mpResult.NextPaymentDate.HasValue)
            {
                localSubscription.NextPaymentDate = mpResult.NextPaymentDate;
            }
                
            // 3. Salva no banco
            await repository.UpdateAsync(localSubscription).ConfigureAwait(false);
            
            await transaction.CommitAsync().ConfigureAwait(false);
            logger.LogInformation("Assinatura local {ExternalId} atualizada com sucesso.", externalId);

            return mpResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            logger.LogError(ex, "Rollback executado ao atualizar assinatura ExternalId: {ExternalId}", externalId);
            throw;
        }
    }

    public async Task<SubscriptionResponse?> GetSubscriptionStatusAsync(Guid userId)
    {
        var cacheResult = await repository.GetByUserIdAsync(userId).ConfigureAwait(false);
        var localData = cacheResult.Data;

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