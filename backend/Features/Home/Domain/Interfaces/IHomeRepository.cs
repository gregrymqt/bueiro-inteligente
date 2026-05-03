using backend.Features.Home.Domain.Entities;

namespace backend.Features.Home.Domain.Interfaces;

public interface IHomeRepository
{
    Task<HomeContent> GetAllContentAsync(CancellationToken ct = default);

    Task<IReadOnlyList<CarouselModel>> GetAllCarouselsAsync(CancellationToken ct = default);
    Task<CarouselModel?> GetCarouselByIdAsync(Guid carouselId, CancellationToken ct = default);
    Task<CarouselModel> CreateCarouselAsync(CarouselModel carousel, CancellationToken ct = default);
    Task<CarouselModel> UpdateCarouselAsync(CarouselModel carousel, CancellationToken ct = default);
    Task DeleteCarouselAsync(CarouselModel carousel, CancellationToken ct = default);

    Task<IReadOnlyList<StatCardModel>> GetAllStatCardsAsync(CancellationToken ct = default);
    Task<StatCardModel?> GetStatCardByIdAsync(Guid statCardId, CancellationToken ct = default);
    Task<StatCardModel> CreateStatCardAsync(StatCardModel statCard, CancellationToken ct = default);
    Task<StatCardModel> UpdateStatCardAsync(StatCardModel statCard, CancellationToken ct = default);
    Task DeleteStatCardAsync(StatCardModel statCard, CancellationToken ct = default);
}
