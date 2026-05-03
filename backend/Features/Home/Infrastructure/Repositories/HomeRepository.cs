using backend.Core;
using backend.Features.Home.Domain;
using backend.Features.Home.Domain.Entities;
using backend.Features.Home.Domain.Interfaces;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Home.Infrastructure.Repositories;

public sealed class HomeRepository(
    AppDbContext dbContext,
    ILogger<HomeRepository> logger,
    ICacheService cache) // 1. Injetamos o seu ICacheService aqui
    : IHomeRepository
{
    // 2. Definição das chaves de cache e tempo de expiração
    private const string CacheKeyAllCarousels = "home:carousels:all";
    private const string CacheKeyAllStatCards = "home:statcards:all";
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromHours(1);

    private static string GetCarouselCacheKey(Guid id) => $"home:carousels:{id}";
    private static string GetStatCardCacheKey(Guid id) => $"home:statcards:{id}";

    public async Task<HomeContent> GetAllContentAsync(CancellationToken ct = default)
    {
        // Como os métodos abaixo agora têm cache, este método fica extremamente rápido
        var carousels = await GetAllCarouselsAsync(ct).ConfigureAwait(false);
        var stats = await GetAllStatCardsAsync(ct).ConfigureAwait(false);
        return new HomeContent(carousels, stats);
    }

    #region Carousel Persistence

    public async Task<IReadOnlyList<CarouselModel>> GetAllCarouselsAsync(
        CancellationToken ct = default
    )
    {
        try
        {
            // 3. Utilizamos o GetOrSetAsync e retornamos apenas o .Data para respeitar a interface
            var cacheResult = await cache.GetOrSetAsync<IReadOnlyList<CarouselModel>>(
                CacheKeyAllCarousels,
                async () => await dbContext.HomeCarousels.AsNoTracking()
                    .Include(c => c.Upload)
                    .OrderBy(c => c.Order)
                    .ThenBy(c => c.Id)
                    .ToListAsync(ct)
                    .ConfigureAwait(false),
                DefaultCacheTtl
            );

            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving home carousel items.");
            throw new ConnectionException(
                "HomeRepository.GetAllCarouselsAsync",
                "Failed to query carousels.",
                ex
            );
        }
    }

    public async Task<CarouselModel?> GetCarouselByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync<CarouselModel?>(
                GetCarouselCacheKey(id),
                async () => await dbContext.HomeCarousels.AsNoTracking()
                    .Include(c => c.Upload)
                    .FirstOrDefaultAsync(c => c.Id == id, ct)
                    .ConfigureAwait(false),
                DefaultCacheTtl
            );

            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving carousel {Id}", id);
            throw new ConnectionException(
                "HomeRepository.GetCarouselByIdAsync",
                $"Failed to query carousel '{id}'.",
                ex
            );
        }
    }

    public async Task<CarouselModel> CreateCarouselAsync(
        CarouselModel c,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(c);
        try
        {
            await dbContext.HomeCarousels.AddAsync(c, ct).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            // 4. Invalidação do cache: ao criar, a lista geral de carrosséis fica desatualizada
            await cache.RemoveAsync(CacheKeyAllCarousels);

            return c;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating carousel.");
            throw new ConnectionException(
                "HomeRepository.CreateCarouselAsync",
                "Failed to create carousel.",
                ex
            );
        }
    }

    public async Task<CarouselModel> UpdateCarouselAsync(
        CarouselModel c,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(c);
        try
        {
            dbContext.HomeCarousels.Update(c);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            // 4. Invalidação: Remove a lista geral e o item específico
            await cache.RemoveAsync(CacheKeyAllCarousels);
            await cache.RemoveAsync(GetCarouselCacheKey(c.Id));

            return c;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating carousel {Id}", c.Id);
            throw new ConnectionException(
                "HomeRepository.UpdateCarouselAsync",
                $"Failed to update carousel '{c.Id}'.",
                ex
            );
        }
    }

    public async Task DeleteCarouselAsync(CarouselModel c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        try
        {
            dbContext.HomeCarousels.Remove(c);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            // 4. Invalidação: Remove a lista geral e o item específico
            await cache.RemoveAsync(CacheKeyAllCarousels);
            await cache.RemoveAsync(GetCarouselCacheKey(c.Id));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting carousel {Id}", c.Id);
            throw new ConnectionException(
                "HomeRepository.DeleteCarouselAsync",
                $"Failed to delete carousel '{c.Id}'.",
                ex
            );
        }
    }

    #endregion

    #region StatCard Persistence

    public async Task<IReadOnlyList<StatCardModel>> GetAllStatCardsAsync(
        CancellationToken ct = default
    )
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync<IReadOnlyList<StatCardModel>>(
                CacheKeyAllStatCards,
                async () => await dbContext.HomeStats.AsNoTracking()
                    .OrderBy(s => s.Order)
                    .ThenBy(s => s.Id)
                    .ToListAsync(ct)
                    .ConfigureAwait(false),
                DefaultCacheTtl
            );

            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving stat cards.");
            throw new ConnectionException(
                "HomeRepository.GetAllStatCardsAsync",
                "Failed to query stats.",
                ex
            );
        }
    }

    public async Task<StatCardModel?> GetStatCardByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var cacheResult = await cache.GetOrSetAsync<StatCardModel?>(
                GetStatCardCacheKey(id),
                async () => await dbContext.HomeStats.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id, ct)
                    .ConfigureAwait(false),
                DefaultCacheTtl
            );

            return cacheResult.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving stat card {Id}", id);
            throw new ConnectionException(
                "HomeRepository.GetStatCardByIdAsync",
                $"Failed to query stat card '{id}'.",
                ex
            );
        }
    }

    public async Task<StatCardModel> CreateStatCardAsync(
        StatCardModel s,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(s);
        try
        {
            await dbContext.HomeStats.AddAsync(s, ct).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            await cache.RemoveAsync(CacheKeyAllStatCards);

            return s;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating stat card.");
            throw new ConnectionException(
                "HomeRepository.CreateStatCardAsync",
                "Failed to create stat card.",
                ex
            );
        }
    }

    public async Task<StatCardModel> UpdateStatCardAsync(
        StatCardModel s,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(s);
        try
        {
            dbContext.HomeStats.Update(s);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            await cache.RemoveAsync(CacheKeyAllStatCards);
            await cache.RemoveAsync(GetStatCardCacheKey(s.Id));

            return s;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating stat card {Id}", s.Id);
            throw new ConnectionException(
                "HomeRepository.UpdateStatCardAsync",
                $"Failed to update stat card '{s.Id}'.",
                ex
            );
        }
    }

    public async Task DeleteStatCardAsync(StatCardModel s, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(s);
        try
        {
            dbContext.HomeStats.Remove(s);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

            await cache.RemoveAsync(CacheKeyAllStatCards);
            await cache.RemoveAsync(GetStatCardCacheKey(s.Id));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting stat card {Id}", s.Id);
            throw new ConnectionException(
                "HomeRepository.DeleteStatCardAsync",
                $"Failed to delete stat card '{s.Id}'.",
                ex
            );
        }
    }

    #endregion
}