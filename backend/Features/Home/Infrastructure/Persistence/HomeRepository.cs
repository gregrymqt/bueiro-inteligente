using backend.Core;
using backend.Features.Home.Domain;
using backend.Features.Home.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Home.Infrastructure.Persistence;

public sealed class HomeRepository(AppDbContext dbContext, ILogger<HomeRepository> logger)
    : IHomeRepository
{
    public async Task<HomeContent> GetAllContentAsync(CancellationToken ct = default)
    {
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
            return await dbContext
                .HomeCarousels.AsNoTracking()
                .OrderBy(c => c.Order)
                .ThenBy(c => c.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);
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
            return await dbContext
                .HomeCarousels.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct)
                .ConfigureAwait(false);
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
            return await dbContext
                .HomeStats.AsNoTracking()
                .OrderBy(s => s.Order)
                .ThenBy(s => s.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);
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
            return await dbContext
                .HomeStats.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct)
                .ConfigureAwait(false);
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
