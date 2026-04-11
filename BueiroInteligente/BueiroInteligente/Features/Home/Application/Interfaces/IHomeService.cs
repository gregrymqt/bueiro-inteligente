using BueiroInteligente.Features.Home.Application.DTOs;

namespace BueiroInteligente.Features.Home.Application.Interfaces;

/// <summary>
/// Defines the Home use-cases exposed to the presentation layer.
/// </summary>
public interface IHomeService
{
    /// <summary>Gets the full Home page content.</summary>
    Task<HomeResponseDto> GetHomeContentAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets all carousel items as API responses.</summary>
    Task<IReadOnlyList<CarouselResponseDto>> GetAllCarouselsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a carousel item by its identifier.</summary>
    Task<CarouselResponseDto> GetCarouselByIdAsync(Guid carouselId, CancellationToken cancellationToken = default);

    /// <summary>Creates a carousel item.</summary>
    Task<CarouselResponseDto> CreateCarouselAsync(CarouselCreateDto request, CancellationToken cancellationToken = default);

    /// <summary>Updates a carousel item.</summary>
    Task<CarouselResponseDto> UpdateCarouselAsync(Guid carouselId, CarouselUpdateDto request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a carousel item.</summary>
    Task DeleteCarouselAsync(Guid carouselId, CancellationToken cancellationToken = default);

    /// <summary>Gets all statistic cards as API responses.</summary>
    Task<IReadOnlyList<StatCardResponseDto>> GetAllStatCardsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a statistic card by its identifier.</summary>
    Task<StatCardResponseDto> GetStatCardByIdAsync(Guid statCardId, CancellationToken cancellationToken = default);

    /// <summary>Creates a statistic card.</summary>
    Task<StatCardResponseDto> CreateStatCardAsync(StatCardCreateDto request, CancellationToken cancellationToken = default);

    /// <summary>Updates a statistic card.</summary>
    Task<StatCardResponseDto> UpdateStatCardAsync(Guid statCardId, StatCardUpdateDto request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a statistic card.</summary>
    Task DeleteStatCardAsync(Guid statCardId, CancellationToken cancellationToken = default);
}