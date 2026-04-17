using backend.Features.Home.Application.DTOs;

namespace backend.Features.Home.Application.Interfaces;

public interface IHomeService
{
    Task<HomeResponseDto> GetHomeContentAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CarouselResponseDto>> GetAllCarouselsAsync(CancellationToken ct = default);
    Task<CarouselResponseDto> GetCarouselByIdAsync(Guid carouselId, CancellationToken ct = default);
    Task<CarouselResponseDto> CreateCarouselAsync(CarouselCreateDto request, CancellationToken ct = default);
    Task<CarouselResponseDto> UpdateCarouselAsync(Guid carouselId, CarouselUpdateDto request, CancellationToken ct = default);
    Task DeleteCarouselAsync(Guid carouselId, CancellationToken ct = default);
    Task<IReadOnlyList<StatCardResponseDto>> GetAllStatCardsAsync(CancellationToken ct = default);
    Task<StatCardResponseDto> GetStatCardByIdAsync(Guid statCardId, CancellationToken ct = default);
    Task<StatCardResponseDto> CreateStatCardAsync(StatCardCreateDto request, CancellationToken ct = default);
    Task<StatCardResponseDto> UpdateStatCardAsync(Guid statCardId, StatCardUpdateDto request, CancellationToken ct = default);
    Task DeleteStatCardAsync(Guid statCardId, CancellationToken ct = default);
}