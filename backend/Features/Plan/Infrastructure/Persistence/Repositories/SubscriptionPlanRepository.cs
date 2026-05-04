using backend.Features.Subscription.Domain.Entities;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Subscription.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionPlanRepository(
    AppDbContext context,
    ICacheService cacheService,
    ILogger<SubscriptionPlanRepository> logger
) : ISubscriptionPlanRepository
{
    // Define um TTL de 12 horas para os planos, já que são alterados com pouca frequência
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(12);

    private const string ActivePlansCacheKey = "subscription_plans:active";

    public async Task<CacheResponseDto<SubscriptionPlan?>> GetByIdAsync(Guid id)
    {
        string cacheKey = $"subscription_plan:id:{id}";

        try
        {
            return await cacheService.GetOrSetAsync(
                cacheKey,
                async () => await context.SubscriptionPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id)
                    .ConfigureAwait(false),
                CacheExpiry
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar plano de assinatura pelo ID: {Id}", id);
            throw;
        }
    }

    public async Task<CacheResponseDto<SubscriptionPlan?>> GetByExternalIdAsync(string externalId)
    {
        string cacheKey = $"subscription_plan:external_id:{externalId}";

        try
        {
            return await cacheService.GetOrSetAsync(
                cacheKey,
                async () => await context.SubscriptionPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ExternalId == externalId)
                    .ConfigureAwait(false),
                CacheExpiry
            ).ConfigureAwait(false); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar plano pelo ID externo MP: {ExternalId}", externalId);
            throw;
        }
    }

    public async Task<CacheResponseDto<IEnumerable<SubscriptionPlan>>> GetAllActiveAsync()
    {
        try
        {
            return await cacheService.GetOrSetAsync(
                ActivePlansCacheKey,
                async () =>
                {
                    var plans = await context.SubscriptionPlans
                        .AsNoTracking()
                        .Where(x => x.Status == "active")
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return plans.AsEnumerable();
                },
                CacheExpiry
            ).ConfigureAwait(false); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao listar planos de assinatura ativos.");
            throw;
        }
    }

    public async Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan)
    {
        try
        {
            plan.DateCreated = DateTime.UtcNow;
            plan.LastModified = DateTime.UtcNow;

            await context.SubscriptionPlans.AddAsync(plan).ConfigureAwait(false); 
            await context.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Plano de assinatura '{PlanName}' ({ExternalId}) criado com sucesso.", plan.Name, plan.ExternalId);

            // Invalida o cache da lista global de planos, forçando uma nova consulta do PostgreSQL na próxima vez
            await cacheService.RemoveAsync(ActivePlansCacheKey).ConfigureAwait(false);

            return plan;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar novo plano de assinatura no banco: {Name}", plan.Name);
            throw;
        }
    }

    public async Task UpdateAsync(SubscriptionPlan plan)
    {
        try
        {
            plan.LastModified = DateTime.UtcNow;

            context.SubscriptionPlans.Update(plan); 
            await context.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Plano de assinatura '{PlanName}' ({ExternalId}) atualizado.", plan.Name, plan.ExternalId);

            // Invalidação granular de cache: remove tanto as buscas diretas quanto a lista global[cite: 21, 22]
            await Task.WhenAll(
                cacheService.RemoveAsync($"subscription_plan:id:{plan.Id}"),
                cacheService.RemoveAsync($"subscription_plan:external_id:{plan.ExternalId}"),
                cacheService.RemoveAsync(ActivePlansCacheKey)
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar plano de assinatura: {Id}", plan.Id);
            throw;
        }
    }
}