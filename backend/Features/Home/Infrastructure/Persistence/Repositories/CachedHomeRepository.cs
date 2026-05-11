using backend.Features.Home.Domain;
using backend.Features.Home.Domain.Entities;
using backend.Features.Home.Domain.Interfaces;
using backend.Infrastructure.Cache;

namespace backend.Features.Home.Infrastructure.Persistence.Repositories;

public sealed class CachedHomeRepository(
    IHomeRepository decorated,
    ICacheService cache)
    : IHomeRepository
{
    private const string CacheKeyAllCarousels = "home:carousels:all";
    private const string CacheKeyAllStatCards = "home:statcards:all";
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromHours(1);

    private static string GetCarouselCacheKey(Guid id) => $"home:carousels:{id}";
    private static string GetStatCardCacheKey(Guid id) => $"home:statcards:{id}";

    public async Task<HomeContent> GetAllContentAsync(CancellationToken ct = default)
    {
        var carousels = await GetAllCarouselsAsync(ct).ConfigureAwait(false);
        var stats = await GetAllStatCardsAsync(ct).ConfigureAwait(false);
        return new HomeContent(carousels, stats);
    }

    public async Task<IReadOnlyList<CarouselModel>> GetAllCarouselsAsync(CancellationToken ct = default)
    {
        var cacheResult = await cache.GetOrSetAsync(
            CacheKeyAllCarousels,
            async () => await decorated.GetAllCarouselsAsync(ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<CarouselModel?> GetCarouselByIdAsync(Guid carouselId, CancellationToken ct = default)
    {
        var cacheResult = await cache.GetOrSetAsync(
            GetCarouselCacheKey(carouselId),
            async () => await decorated.GetCarouselByIdAsync(carouselId, ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<CarouselModel> CreateCarouselAsync(CarouselModel carousel, CancellationToken ct = default)
    {
        var result = await decorated.CreateCarouselAsync(carousel, ct).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllCarousels);
        return result;
    }

    public async Task<CarouselModel> UpdateCarouselAsync(CarouselModel carousel, CancellationToken ct = default)
    {
        var result = await decorated.UpdateCarouselAsync(carousel, ct).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllCarousels);
        await cache.RemoveAsync(GetCarouselCacheKey(carousel.Id));
        return result;
    }

    public async Task DeleteCarouselAsync(CarouselModel carousel, CancellationToken ct = default)
    {
        await decorated.DeleteCarouselAsync(carousel, ct).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllCarousels);
        await cache.RemoveAsync(GetCarouselCacheKey(carousel.Id));
    }

    public async Task<IReadOnlyList<StatCardModel>> GetAllStatCardsAsync(CancellationToken ct = default)
    {
        var cacheResult = await cache.GetOrSetAsync(
            CacheKeyAllStatCards,
            async () => await decorated.GetAllStatCardsAsync(ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<StatCardModel?> GetStatCardByIdAsync(Guid statCardId, CancellationToken ct = default)
    {
        var cacheResult = await cache.GetOrSetAsync(
            GetStatCardCacheKey(statCardId),
            async () => await decorated.GetStatCardByIdAsync(statCardId, ct).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<StatCardModel> CreateStatCardAsync(StatCardModel statCard, CancellationToken ct = default)
    {
        var result = await decorated.CreateStatCardAsync(statCard, ct).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllStatCards);
        return result;
    }

    public async Task<StatCardModel> UpdateStatCardAsync(StatCardModel statCard, CancellationToken ct = default)
    {
        var result = await decorated.UpdateStatCardAsync(statCard, ct).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllStatCards);
        await cache.RemoveAsync(GetStatCardCacheKey(statCard.Id));
        return result;
    }

    public async Task DeleteStatCardAsync(StatCardModel statCard, CancellationToken ct = default)
    {
        await decorated.DeleteStatCardAsync(statCard, ct).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllStatCards);
        await cache.RemoveAsync(GetStatCardCacheKey(statCard.Id));
    }
}
