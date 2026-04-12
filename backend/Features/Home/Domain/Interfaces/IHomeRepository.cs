using backend.Features.Home.Domain;

namespace backend.Features.Home.Domain.Interfaces;

/// <summary>
/// Defines persistence operations for Home carousel and statistic card content.
/// </summary>
public interface IHomeRepository
{
    /// <summary>Gets the full Home content, including carousel items and statistic cards.</summary>
    Task<HomeContent> GetAllContentAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets all carousel items ordered by their display order.</summary>
    Task<IReadOnlyList<CarouselModel>> GetAllCarouselsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a carousel item by its identifier.</summary>
    Task<CarouselModel?> GetCarouselByIdAsync(Guid carouselId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new carousel item.</summary>
    Task<CarouselModel> CreateCarouselAsync(CarouselModel carousel, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing carousel item.</summary>
    Task<CarouselModel> UpdateCarouselAsync(CarouselModel carousel, CancellationToken cancellationToken = default);

    /// <summary>Deletes a carousel item.</summary>
    Task DeleteCarouselAsync(CarouselModel carousel, CancellationToken cancellationToken = default);

    /// <summary>Gets all statistic cards ordered by their display order.</summary>
    Task<IReadOnlyList<StatCardModel>> GetAllStatCardsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a statistic card by its identifier.</summary>
    Task<StatCardModel?> GetStatCardByIdAsync(Guid statCardId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new statistic card.</summary>
    Task<StatCardModel> CreateStatCardAsync(StatCardModel statCard, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing statistic card.</summary>
    Task<StatCardModel> UpdateStatCardAsync(StatCardModel statCard, CancellationToken cancellationToken = default);

    /// <summary>Deletes a statistic card.</summary>
    Task DeleteStatCardAsync(StatCardModel statCard, CancellationToken cancellationToken = default);
}