using backend.Features.Subscription.Domain.Entities;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Subscription.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository(
    AppDbContext context,
    ILogger<SubscriptionRepository> logger
) : ISubscriptionRepository
{
    public async Task<UserSubscription?> GetByIdAsync(Guid id)
    {
        try
        {
            return await context.UserSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar assinatura pelo ID interno: {Id}", id);
            throw;
        }
    }

    public async Task<UserSubscription?> GetByExternalIdAsync(string externalId)
    {
        try
        {
            // Muito acessado via Webhooks, portanto, a model deve ter Index em ExternalId
            return await context.UserSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ExternalId == externalId)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar assinatura pelo ID externo MP: {ExternalId}", externalId);
            throw;
        }
    }

    public async Task<UserSubscription?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            return await context.UserSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar assinatura do usuário: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSubscription> CreateAsync(UserSubscription subscription)
    {
        try
        {
            subscription.DateCreated = DateTime.UtcNow;
            subscription.LastModified = DateTime.UtcNow;

            await context.UserSubscriptions.AddAsync(subscription).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Assinatura {ExternalId} criada para o usuário {UserId}.",
                subscription.ExternalId, subscription.UserId);

            return subscription;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao persistir nova assinatura para o usuário: {UserId}", subscription.UserId);
            throw;
        }
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        try
        {
            subscription.LastModified = DateTime.UtcNow;

            context.UserSubscriptions.Update(subscription);
            await context.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Assinatura {ExternalId} atualizada com sucesso.", subscription.ExternalId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar assinatura: {Id}", subscription.Id);
            throw;
        }
    }
}