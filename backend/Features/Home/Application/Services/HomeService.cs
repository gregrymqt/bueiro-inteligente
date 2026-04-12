using backend.Core;
using HomeDtos = backend.Features.Home.Application.DTOs;
using HomeDomain = backend.Features.Home.Domain;
using backend.Features.Home.Application.Interfaces;
using backend.Features.Home.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Home.Application.Services;

/// <summary>
/// Implements the Home use-cases.
/// </summary>
public sealed class HomeService(IHomeRepository homeRepository, ILogger<HomeService> logger)
    : IHomeService
{
    private readonly IHomeRepository _homeRepository =
        homeRepository ?? throw new ArgumentNullException(nameof(homeRepository));

    private readonly ILogger<HomeService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<HomeDtos.HomeResponseDto> GetHomeContentAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Loading home content.");

        HomeDomain.HomeContent content = await _homeRepository
            .GetAllContentAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HomeDtos.HomeResponseDto(
            content.Carousels.Select(MapToCarouselResponse).ToList(),
            content.Stats.Select(MapToStatCardResponse).ToList()
        );
    }

    public async Task<IReadOnlyList<HomeDtos.CarouselResponseDto>> GetAllCarouselsAsync(
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<HomeDomain.CarouselModel> carousels = await _homeRepository
            .GetAllCarouselsAsync(cancellationToken)
            .ConfigureAwait(false);

        return carousels.Select(MapToCarouselResponse).ToList();
    }

    public async Task<HomeDtos.CarouselResponseDto> GetCarouselByIdAsync(
        Guid carouselId,
        CancellationToken cancellationToken = default
    )
    {
        HomeDomain.CarouselModel? carousel = await _homeRepository
            .GetCarouselByIdAsync(carouselId, cancellationToken)
            .ConfigureAwait(false);

        if (carousel is null)
        {
            throw new NotFoundException("Carousel", carouselId);
        }

        return MapToCarouselResponse(carousel);
    }

    public async Task<HomeDtos.CarouselResponseDto> CreateCarouselAsync(
        HomeDtos.CarouselCreateDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        HomeDomain.CarouselModel carousel = MapToCarouselModel(request);

        HomeDomain.CarouselModel createdCarousel = await _homeRepository
            .CreateCarouselAsync(carousel, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Created home carousel item {CarouselId}.", createdCarousel.Id);
        return MapToCarouselResponse(createdCarousel);
    }

    public async Task<HomeDtos.CarouselResponseDto> UpdateCarouselAsync(
        Guid carouselId,
        HomeDtos.CarouselUpdateDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        HomeDomain.CarouselModel? carousel = await _homeRepository
            .GetCarouselByIdAsync(carouselId, cancellationToken)
            .ConfigureAwait(false);

        if (carousel is null)
        {
            throw new NotFoundException("Carousel", carouselId);
        }

        if (request.Title is not null)
        {
            carousel.Title = NormalizeRequiredText(request.Title, nameof(request.Title));
        }

        if (request.Subtitle is not null)
        {
            carousel.Subtitle = NormalizeOptionalText(request.Subtitle);
        }

        if (request.ImageUrl is not null)
        {
            carousel.ImageUrl = NormalizeRequiredText(request.ImageUrl, nameof(request.ImageUrl));
        }

        if (request.ActionUrl is not null)
        {
            carousel.ActionUrl = NormalizeOptionalText(request.ActionUrl);
        }

        if (request.Order.HasValue)
        {
            carousel.Order = request.Order.Value;
        }

        if (request.Section.HasValue)
        {
            carousel.Section = MapToDomainSection(request.Section.Value);
        }

        HomeDomain.CarouselModel updatedCarousel = await _homeRepository
            .UpdateCarouselAsync(carousel, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Updated home carousel item {CarouselId}.", updatedCarousel.Id);
        return MapToCarouselResponse(updatedCarousel);
    }

    public async Task DeleteCarouselAsync(
        Guid carouselId,
        CancellationToken cancellationToken = default
    )
    {
        HomeDomain.CarouselModel? carousel = await _homeRepository
            .GetCarouselByIdAsync(carouselId, cancellationToken)
            .ConfigureAwait(false);

        if (carousel is null)
        {
            throw new NotFoundException("Carousel", carouselId);
        }

        await _homeRepository.DeleteCarouselAsync(carousel, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Deleted home carousel item {CarouselId}.", carouselId);
    }

    public async Task<IReadOnlyList<HomeDtos.StatCardResponseDto>> GetAllStatCardsAsync(
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<HomeDomain.StatCardModel> statCards = await _homeRepository
            .GetAllStatCardsAsync(cancellationToken)
            .ConfigureAwait(false);

        return statCards.Select(MapToStatCardResponse).ToList();
    }

    public async Task<HomeDtos.StatCardResponseDto> GetStatCardByIdAsync(
        Guid statCardId,
        CancellationToken cancellationToken = default
    )
    {
        HomeDomain.StatCardModel? statCard = await _homeRepository
            .GetStatCardByIdAsync(statCardId, cancellationToken)
            .ConfigureAwait(false);

        if (statCard is null)
        {
            throw new NotFoundException("StatCard", statCardId);
        }

        return MapToStatCardResponse(statCard);
    }

    public async Task<HomeDtos.StatCardResponseDto> CreateStatCardAsync(
        HomeDtos.StatCardCreateDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        HomeDomain.StatCardModel statCard = MapToStatCardModel(request);

        HomeDomain.StatCardModel createdStatCard = await _homeRepository
            .CreateStatCardAsync(statCard, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Created home stat card {StatCardId}.", createdStatCard.Id);
        return MapToStatCardResponse(createdStatCard);
    }

    public async Task<HomeDtos.StatCardResponseDto> UpdateStatCardAsync(
        Guid statCardId,
        HomeDtos.StatCardUpdateDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        HomeDomain.StatCardModel? statCard = await _homeRepository
            .GetStatCardByIdAsync(statCardId, cancellationToken)
            .ConfigureAwait(false);

        if (statCard is null)
        {
            throw new NotFoundException("StatCard", statCardId);
        }

        if (request.Title is not null)
        {
            statCard.Title = NormalizeRequiredText(request.Title, nameof(request.Title));
        }

        if (request.Value is not null)
        {
            statCard.Value = NormalizeRequiredText(request.Value, nameof(request.Value));
        }

        if (request.Description is not null)
        {
            statCard.Description = NormalizeRequiredText(request.Description, nameof(request.Description));
        }

        if (request.IconName is not null)
        {
            statCard.IconName = NormalizeRequiredText(request.IconName, nameof(request.IconName));
        }

        if (request.Color.HasValue)
        {
            statCard.Color = MapToDomainColor(request.Color.Value);
        }

        if (request.Order.HasValue)
        {
            statCard.Order = request.Order.Value;
        }

        HomeDomain.StatCardModel updatedStatCard = await _homeRepository
            .UpdateStatCardAsync(statCard, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Updated home stat card {StatCardId}.", updatedStatCard.Id);
        return MapToStatCardResponse(updatedStatCard);
    }

    public async Task DeleteStatCardAsync(
        Guid statCardId,
        CancellationToken cancellationToken = default
    )
    {
        HomeDomain.StatCardModel? statCard = await _homeRepository
            .GetStatCardByIdAsync(statCardId, cancellationToken)
            .ConfigureAwait(false);

        if (statCard is null)
        {
            throw new NotFoundException("StatCard", statCardId);
        }

        await _homeRepository.DeleteStatCardAsync(statCard, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Deleted home stat card {StatCardId}.", statCardId);
    }

    private static HomeDtos.CarouselResponseDto MapToCarouselResponse(HomeDomain.CarouselModel carousel)
    {
        return new HomeDtos.CarouselResponseDto(
            carousel.Id,
            carousel.Title,
            carousel.Subtitle,
            carousel.ImageUrl,
            carousel.ActionUrl,
            carousel.Order,
            MapToDtoSection(carousel.Section)
        );
    }

    private static HomeDomain.CarouselModel MapToCarouselModel(HomeDtos.CarouselCreateDto request)
    {
        return new HomeDomain.CarouselModel
        {
            Title = NormalizeRequiredText(request.Title, nameof(request.Title)),
            Subtitle = NormalizeOptionalText(request.Subtitle),
            ImageUrl = NormalizeRequiredText(request.ImageUrl, nameof(request.ImageUrl)),
            ActionUrl = NormalizeOptionalText(request.ActionUrl),
            Order = request.Order,
            Section = MapToDomainSection(request.Section),
        };
    }

    private static HomeDtos.StatCardResponseDto MapToStatCardResponse(HomeDomain.StatCardModel statCard)
    {
        return new HomeDtos.StatCardResponseDto(
            statCard.Id,
            statCard.Title,
            statCard.Value,
            statCard.Description,
            statCard.IconName,
            MapToDtoColor(statCard.Color),
            statCard.Order
        );
    }

    private static HomeDomain.StatCardModel MapToStatCardModel(HomeDtos.StatCardCreateDto request)
    {
        return new HomeDomain.StatCardModel
        {
            Title = NormalizeRequiredText(request.Title, nameof(request.Title)),
            Value = NormalizeRequiredText(request.Value, nameof(request.Value)),
            Description = NormalizeRequiredText(request.Description, nameof(request.Description)),
            IconName = NormalizeRequiredText(request.IconName, nameof(request.IconName)),
            Color = MapToDomainColor(request.Color),
            Order = request.Order,
        };
    }

    private static string NormalizeRequiredText(string? value, string parameterName)
    {
        string? normalizedValue = NormalizeOptionalText(value);

        if (normalizedValue is null)
        {
            throw LogicException.InvalidValue(parameterName, value);
        }

        return normalizedValue;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static HomeDomain.CarouselSection MapToDomainSection(HomeDtos.CarouselSection section)
    {
        return Enum.Parse<HomeDomain.CarouselSection>(section.ToString(), ignoreCase: false);
    }

    private static HomeDtos.CarouselSection MapToDtoSection(HomeDomain.CarouselSection section)
    {
        return Enum.Parse<HomeDtos.CarouselSection>(section.ToString(), ignoreCase: false);
    }

    private static HomeDomain.StatCardColor MapToDomainColor(HomeDtos.StatCardColor color)
    {
        return Enum.Parse<HomeDomain.StatCardColor>(color.ToString(), ignoreCase: false);
    }

    private static HomeDtos.StatCardColor MapToDtoColor(HomeDomain.StatCardColor color)
    {
        return Enum.Parse<HomeDtos.StatCardColor>(color.ToString(), ignoreCase: false);
    }
}