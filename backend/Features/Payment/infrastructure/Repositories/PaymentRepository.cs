using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence; // Namespace do seu AppDbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Infrastructure.Persistence;

public sealed class PaymentRepository(
    AppDbContext context,
    ICacheService cache,
    ILogger<PaymentRepository> logger
) : IPaymentRepository
{
    private readonly AppDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<PaymentRepository> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private static string GetCacheKey(Guid id) => $"payment:transaction:{id}";

    public async Task AddAsync(PaymentTransaction transaction)
    {
        try
        {
            _logger.LogInformation(
                "Persistindo transação de pagamento no banco: {Id}",
                transaction.Id
            );
            await _context.Set<PaymentTransaction>().AddAsync(transaction);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao persistir transação {Id} no PostgreSQL.", transaction.Id);
            throw;
        }
    }

    public async Task UpdateAsync(PaymentTransaction transaction)
    {
        try
        {
            _logger.LogInformation(
                "Atualizando transação {Id} (Status: {Status})",
                transaction.Id,
                transaction.Status
            );

            _context.Set<PaymentTransaction>().Update(transaction);
            await _context.SaveChangesAsync();

            // Invalida o cache após atualização para manter consistência
            await _cache.RemoveAsync(GetCacheKey(transaction.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha ao atualizar transação {Id} no banco de dados.",
                transaction.Id
            );
            throw;
        }
    }

    public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
    {
        try
        {
            var key = GetCacheKey(id);

            // Tenta obter do cache, se falhar, busca no banco e alimenta o cache
            var response = await _cache.GetOrSetAsync(
                key,
                async () =>
                    await _context.Set<PaymentTransaction>().FirstOrDefaultAsync(p => p.Id == id),
                TimeSpan.FromMinutes(15) // TTL de 15 minutos
            );

            return response.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na camada de dados ao buscar transação por ID: {Id}", id);
            // Fallback direto no banco caso o Redis apresente instabilidade
            return await _context.Set<PaymentTransaction>().FirstOrDefaultAsync(p => p.Id == id);
        }
    }

    public async Task<PaymentTransaction?> GetByPaymentIdAsync(long paymentId)
    {
        try
        {
            _logger.LogDebug("Buscando transação pelo ID de Pagamento MP: {PaymentId}", paymentId);
            return await _context
                .Set<PaymentTransaction>()
                .FirstOrDefaultAsync(p => p.MercadoPagoPaymentId == paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao buscar transação pelo MercadoPagoPaymentId: {PaymentId}",
                paymentId
            );
            throw;
        }
    }

    public async Task<PaymentTransaction?> GetByOrderIdAsync(string orderId)
    {
        try
        {
            _logger.LogDebug("Buscando transação pelo ID de Ordem MP: {OrderId}", orderId);
            return await _context
                .Set<PaymentTransaction>()
                .FirstOrDefaultAsync(p => p.MercadoPagoOrderId == orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao buscar transação pelo MercadoPagoOrderId: {OrderId}",
                orderId
            );
            throw;
        }
    }
}
