using backend.Features.Feedbacks.Application.DTOs;
using backend.Features.Feedbacks.Domain.Entities;
using backend.Features.Feedbacks.Domain.Interfaces;
using backend.Infrastructure.Cache;

namespace backend.Features.Feedbacks.Infrastructure.Persistence.Repositories;

public sealed class CachedFeedbackRepository(
    IFeedbackRepository decorated,
    ICacheService cache)
    : IFeedbackRepository
{
    private const string CacheKeyAllFeedbacks = "feedbacks:all";
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromHours(1);

    private static string GetFeedbackCacheKey(Guid id) => $"feedbacks:{id}";

    public async Task<IEnumerable<FeedbackResponseDTO>> GetAllAsync()
    {
        var cacheResult = await cache.GetOrSetAsync(
            CacheKeyAllFeedbacks,
            async () => await decorated.GetAllAsync().ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task<Feedback?> GetByIdAsync(Guid id)
    {
        var cacheResult = await cache.GetOrSetAsync(
            GetFeedbackCacheKey(id),
            async () => await decorated.GetByIdAsync(id).ConfigureAwait(false),
            DefaultCacheTtl
        );
        return cacheResult.Data;
    }

    public async Task AddAsync(Feedback feedback)
    {
        await decorated.AddAsync(feedback).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllFeedbacks);
    }

    public async Task UpdateAsync(Feedback feedback)
    {
        await decorated.UpdateAsync(feedback).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllFeedbacks);
        await cache.RemoveAsync(GetFeedbackCacheKey(feedback.Id));
    }

    public async Task DeleteAsync(Guid id)
    {
        await decorated.DeleteAsync(id).ConfigureAwait(false);
        await cache.RemoveAsync(CacheKeyAllFeedbacks);
        await cache.RemoveAsync(GetFeedbackCacheKey(id));
    }
}
