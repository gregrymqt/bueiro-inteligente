using backend.Features.Payment.Domain.Entities;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Cache;
using Microsoft.IdentityModel.Tokens;

namespace backend.Features.Payment.infrastructure.Persistence.Repositories;

public sealed class CachedPaymentRepository(
    IPaymentRepository decorated,
    ICacheService cache,
    ILogger<CachedPaymentRepository> logger) : IPaymentRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    private static string GetCacheKeyId(Guid id) => $"payment:id:{id}";
    private static string GetCacheKeyPaymentId(string paymentId) => $"payment:mp_id:{paymentId}";
    private static string GetCacheKeyOrderId(string orderId) => $"payment:order_id:{orderId}";

    public async Task AddAsync(PaymentTransaction transaction)
    {
        await decorated.AddAsync(transaction).ConfigureAwait(false);
        await InvalidateCacheAsync(transaction);
    }

    public async Task UpdateAsync(PaymentTransaction transaction)
    {
        await InvalidateCacheAsync(transaction);
        await decorated.UpdateAsync(transaction).ConfigureAwait(false);
    }

    public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync(
                GetCacheKeyId(id),
                () => decorated.GetByIdAsync(id),
                CacheTtl
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no cache para Payment GetByIdAsync. Consultando banco.");
            return await decorated.GetByIdAsync(id).ConfigureAwait(false);
        }
    }

    public async Task<PaymentTransaction?> GetByPaymentIdAsync(string paymentId)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync(
                GetCacheKeyPaymentId(paymentId),
                () => decorated.GetByPaymentIdAsync(paymentId),
                CacheTtl
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no cache para Payment GetByPaymentIdAsync. Consultando banco.");
            return await decorated.GetByPaymentIdAsync(paymentId).ConfigureAwait(false);
        }
    }

    public async Task<PaymentTransaction?> GetByOrderIdAsync(string orderId)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync(
                GetCacheKeyOrderId(orderId),
                () => decorated.GetByOrderIdAsync(orderId),
                CacheTtl
            );
            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no cache para Payment GetByOrderIdAsync. Consultando banco.");
            return await decorated.GetByOrderIdAsync(orderId).ConfigureAwait(false);
        }
    }

    private async Task InvalidateCacheAsync(PaymentTransaction transaction)
    {
        try
        {
            await cache.RemoveAsync(GetCacheKeyId(transaction.Id));

            if (!string.IsNullOrEmpty(transaction.MercadoPagoPaymentId))
            {
                await cache.RemoveAsync(GetCacheKeyPaymentId(transaction.MercadoPagoPaymentId));
            }

            if (!string.IsNullOrEmpty(transaction.MercadoPagoOrderId))
            {
                await cache.RemoveAsync(GetCacheKeyOrderId(transaction.MercadoPagoOrderId));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao invalidar cache de pagamento.");
        }
    }
}