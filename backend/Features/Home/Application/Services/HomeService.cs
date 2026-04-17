using backend.Core;
using HomeDtos = backend.Features.Home.Application.DTOs;
using HomeDomain = backend.Features.Home.Domain;
using backend.Features.Home.Application.Interfaces;
using backend.Features.Home.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Home.Application.Services;

public sealed class HomeService(IHomeRepository homeRepository, ILogger<HomeService> logger) : IHomeService
{
    private readonly IHomeRepository _homeRepository = homeRepository ?? throw new ArgumentNullException(nameof(homeRepository));
    private readonly ILogger<HomeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<HomeDtos.HomeResponseDto> GetHomeContentAsync(CancellationToken ct = default)
    {
        var content = await _homeRepository.GetAllContentAsync(ct).ConfigureAwait(false);
        return new HomeDtos.HomeResponseDto(
            [.. content.Carousels.Select(MapToCarouselResponse)],
            [.. content.Stats.Select(MapToStatCardResponse)]
        );
    }

    #region Carousel Operations

    public async Task<IReadOnlyList<HomeDtos.CarouselResponseDto>> GetAllCarouselsAsync(CancellationToken ct = default)
    {
        var carousels = await _homeRepository.GetAllCarouselsAsync(ct).ConfigureAwait(false);
        return [.. carousels.Select(MapToCarouselResponse)];
    }

    public async Task<HomeDtos.CarouselResponseDto> GetCarouselByIdAsync(Guid carouselId, CancellationToken ct = default)
    {
        var carousel = await _homeRepository.GetCarouselByIdAsync(carouselId, ct).ConfigureAwait(false) 
                       ?? throw new NotFoundException("Carousel", carouselId);
        return MapToCarouselResponse(carousel);
    }

    public async Task<HomeDtos.CarouselResponseDto> CreateCarouselAsync(HomeDtos.CarouselCreateDto request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var created = await _homeRepository.CreateCarouselAsync(MapToCarouselModel(request), ct).ConfigureAwait(false);
        _logger.LogInformation("Carousel created: {CarouselId}", created.Id);
        return MapToCarouselResponse(created);
    }

    public async Task<HomeDtos.CarouselResponseDto> UpdateCarouselAsync(Guid id, HomeDtos.CarouselUpdateDto req, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(req);
        var c = await _homeRepository.GetCarouselByIdAsync(id, ct).ConfigureAwait(false) ?? throw new NotFoundException("Carousel", id);

        c.Order = req.Order ?? c.Order;
        if (req.Section.HasValue) c.Section = MapEnum<HomeDomain.CarouselSection>(req.Section.Value);
        if (req.Title is not null) c.Title = Normalize(req.Title, nameof(req.Title));
        if (req.ImageUrl is not null) c.ImageUrl = Normalize(req.ImageUrl, nameof(req.ImageUrl));
        c.Subtitle = req.Subtitle is not null ? req.Subtitle.Trim() : c.Subtitle;
        c.ActionUrl = req.ActionUrl is not null ? req.ActionUrl.Trim() : c.ActionUrl;

        var updated = await _homeRepository.UpdateCarouselAsync(c, ct).ConfigureAwait(false);
        return MapToCarouselResponse(updated);
    }

    public async Task DeleteCarouselAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _homeRepository.GetCarouselByIdAsync(id, ct).ConfigureAwait(false) ?? throw new NotFoundException("Carousel", id);
        await _homeRepository.DeleteCarouselAsync(c, ct).ConfigureAwait(false);
    }

    #endregion

    #region StatCard Operations

    public async Task<IReadOnlyList<HomeDtos.StatCardResponseDto>> GetAllStatCardsAsync(CancellationToken ct = default)
    {
        var stats = await _homeRepository.GetAllStatCardsAsync(ct).ConfigureAwait(false);
        return [.. stats.Select(MapToStatCardResponse)];
    }

    public async Task<HomeDtos.StatCardResponseDto> GetStatCardByIdAsync(Guid id, CancellationToken ct = default)
    {
        var stat = await _homeRepository.GetStatCardByIdAsync(id, ct).ConfigureAwait(false) ?? throw new NotFoundException("StatCard", id);
        return MapToStatCardResponse(stat);
    }

    public async Task<HomeDtos.StatCardResponseDto> CreateStatCardAsync(HomeDtos.StatCardCreateDto request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var created = await _homeRepository.CreateStatCardAsync(MapToStatCardModel(request), ct).ConfigureAwait(false);
        return MapToStatCardResponse(created);
    }

    public async Task<HomeDtos.StatCardResponseDto> UpdateStatCardAsync(Guid id, HomeDtos.StatCardUpdateDto req, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(req);
        var s = await _homeRepository.GetStatCardByIdAsync(id, ct).ConfigureAwait(false) ?? throw new NotFoundException("StatCard", id);

        s.Order = req.Order ?? s.Order;
        if (req.Color.HasValue) s.Color = MapEnum<HomeDomain.StatCardColor>(req.Color.Value);
        if (req.Title is not null) s.Title = Normalize(req.Title, nameof(req.Title));
        if (req.Value is not null) s.Value = Normalize(req.Value, nameof(req.Value));
        if (req.Description is not null) s.Description = Normalize(req.Description, nameof(req.Description));
        if (req.IconName is not null) s.IconName = Normalize(req.IconName, nameof(req.IconName));

        var updated = await _homeRepository.UpdateStatCardAsync(s, ct).ConfigureAwait(false);
        return MapToStatCardResponse(updated);
    }

    public async Task DeleteStatCardAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _homeRepository.GetStatCardByIdAsync(id, ct).ConfigureAwait(false) ?? throw new NotFoundException("StatCard", id);
        await _homeRepository.DeleteStatCardAsync(s, ct).ConfigureAwait(false);
    }

    #endregion

    #region Helpers & Mappings

    private static HomeDtos.CarouselResponseDto MapToCarouselResponse(HomeDomain.CarouselModel c) =>
        new(c.Id, c.Title, c.Subtitle, c.ImageUrl, c.ActionUrl, c.Order, MapEnum<HomeDtos.CarouselSection>(c.Section));

    private static HomeDomain.CarouselModel MapToCarouselModel(HomeDtos.CarouselCreateDto r) => new()
    {
        Title = Normalize(r.Title, nameof(r.Title)),
        Subtitle = r.Subtitle?.Trim(),
        ImageUrl = Normalize(r.ImageUrl, nameof(r.ImageUrl)),
        ActionUrl = r.ActionUrl?.Trim(),
        Order = r.Order,
        Section = MapEnum<HomeDomain.CarouselSection>(r.Section)
    };

    private static HomeDtos.StatCardResponseDto MapToStatCardResponse(HomeDomain.StatCardModel s) =>
        new(s.Id, s.Title, s.Value, s.Description, s.IconName, MapEnum<HomeDtos.StatCardColor>(s.Color), s.Order);

    private static HomeDomain.StatCardModel MapToStatCardModel(HomeDtos.StatCardCreateDto r) => new()
    {
        Title = Normalize(r.Title, nameof(r.Title)),
        Value = Normalize(r.Value, nameof(r.Value)),
        Description = Normalize(r.Description, nameof(r.Description)),
        IconName = Normalize(r.IconName, nameof(r.IconName)),
        Color = MapEnum<HomeDomain.StatCardColor>(r.Color),
        Order = r.Order
    };

    private static T MapEnum<T>(object source) where T : struct, Enum => 
        Enum.Parse<T>(source.ToString()!, false);

    private static string Normalize(string value, string param) => 
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw LogicException.InvalidValue(param, value);

    #endregion
}