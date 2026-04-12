using backend.Core;
using backend.Features.Home.Domain;
using backend.Features.Home.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Features.Home.Infrastructure.Persistence;

/// <summary>
/// Persists Home carousel items and statistic cards.
/// </summary>
public sealed class HomeRepository(AppDbContext dbContext, ILogger<HomeRepository> logger)
    : IHomeRepository
{
    private readonly AppDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly ILogger<HomeRepository> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<HomeContent> GetAllContentAsync(
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<CarouselModel> carousels = await GetAllCarouselsAsync(cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<StatCardModel> stats = await GetAllStatCardsAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HomeContent(carousels, stats);
    }

    public async Task<IReadOnlyList<CarouselModel>> GetAllCarouselsAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .HomeCarousels.AsNoTracking()
                .OrderBy(carousel => carousel.Order)
                .ThenBy(carousel => carousel.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving home carousel items.");
            throw new ConnectionException(
                "HomeRepository.GetAllCarouselsAsync",
                "Failed to query home carousel items.",
                exception
            );
        }
    }

    public async Task<CarouselModel?> GetCarouselByIdAsync(
        Guid carouselId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .HomeCarousels.AsNoTracking()
                .FirstOrDefaultAsync(carousel => carousel.Id == carouselId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving home carousel item {CarouselId}", carouselId);
            throw new ConnectionException(
                "HomeRepository.GetCarouselByIdAsync",
                $"Failed to query carousel '{carouselId}'.",
                exception
            );
        }
    }

    public async Task<CarouselModel> CreateCarouselAsync(
        CarouselModel carousel,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(carousel);

        try
        {
            await _dbContext.HomeCarousels.AddAsync(carousel, cancellationToken)
                .ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return carousel;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating home carousel item.");
            throw new ConnectionException(
                "HomeRepository.CreateCarouselAsync",
                $"Failed to create carousel '{carousel.Id}'.",
                exception
            );
        }
    }

    public async Task<CarouselModel> UpdateCarouselAsync(
        CarouselModel carousel,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(carousel);

        try
        {
            _dbContext.HomeCarousels.Update(carousel);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return carousel;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error updating home carousel item {CarouselId}", carousel.Id);
            throw new ConnectionException(
                "HomeRepository.UpdateCarouselAsync",
                $"Failed to update carousel '{carousel.Id}'.",
                exception
            );
        }
    }

    public async Task DeleteCarouselAsync(
        CarouselModel carousel,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(carousel);

        try
        {
            _dbContext.HomeCarousels.Remove(carousel);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error deleting home carousel item {CarouselId}", carousel.Id);
            throw new ConnectionException(
                "HomeRepository.DeleteCarouselAsync",
                $"Failed to delete carousel '{carousel.Id}'.",
                exception
            );
        }
    }

    public async Task<IReadOnlyList<StatCardModel>> GetAllStatCardsAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .HomeStats.AsNoTracking()
                .OrderBy(statCard => statCard.Order)
                .ThenBy(statCard => statCard.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving home statistic cards.");
            throw new ConnectionException(
                "HomeRepository.GetAllStatCardsAsync",
                "Failed to query home statistic cards.",
                exception
            );
        }
    }

    public async Task<StatCardModel?> GetStatCardByIdAsync(
        Guid statCardId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .HomeStats.AsNoTracking()
                .FirstOrDefaultAsync(statCard => statCard.Id == statCardId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving home statistic card {StatCardId}", statCardId);
            throw new ConnectionException(
                "HomeRepository.GetStatCardByIdAsync",
                $"Failed to query statistic card '{statCardId}'.",
                exception
            );
        }
    }

    public async Task<StatCardModel> CreateStatCardAsync(
        StatCardModel statCard,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(statCard);

        try
        {
            await _dbContext.HomeStats.AddAsync(statCard, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return statCard;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating home statistic card.");
            throw new ConnectionException(
                "HomeRepository.CreateStatCardAsync",
                $"Failed to create statistic card '{statCard.Id}'.",
                exception
            );
        }
    }

    public async Task<StatCardModel> UpdateStatCardAsync(
        StatCardModel statCard,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(statCard);

        try
        {
            _dbContext.HomeStats.Update(statCard);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return statCard;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error updating home statistic card {StatCardId}", statCard.Id);
            throw new ConnectionException(
                "HomeRepository.UpdateStatCardAsync",
                $"Failed to update statistic card '{statCard.Id}'.",
                exception
            );
        }
    }

    public async Task DeleteStatCardAsync(
        StatCardModel statCard,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(statCard);

        try
        {
            _dbContext.HomeStats.Remove(statCard);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error deleting home statistic card {StatCardId}", statCard.Id);
            throw new ConnectionException(
                "HomeRepository.DeleteStatCardAsync",
                $"Failed to delete statistic card '{statCard.Id}'.",
                exception
            );
        }
    }
}