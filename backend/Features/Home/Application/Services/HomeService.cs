using backend.Core;
using backend.Features.Home.Application.Interfaces;
using backend.Features.Home.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using backend.Features.Uploads.Domain.Entities;
using HomeDomain = backend.Features.Home.Domain.Entities;
using HomeDtos = backend.Features.Home.Application.DTOs;

namespace backend.Features.Home.Application.Services;

public sealed class HomeService(
    IHomeRepository homeRepository,
    ILogger<HomeService> logger,
    IHttpContextAccessor httpContextAccessor
)
    : IHomeService
{
    private readonly IHomeRepository _homeRepository =
        homeRepository ?? throw new ArgumentNullException(nameof(homeRepository));
    private readonly ILogger<HomeService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public async Task<HomeDtos.HomeResponseDto> GetHomeContentAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Obtendo conteúdo da home.");

        try
        {
            var content = await _homeRepository.GetAllContentAsync(ct).ConfigureAwait(false);
            return new HomeDtos.HomeResponseDto(
                [.. content.Carousels.Select(MapToCarouselResponse)],
                [.. content.Stats.Select(MapToStatCardResponse)]
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter conteúdo da home.");
            throw;
        }
    }

    #region Carousel Operations

    public async Task<IReadOnlyList<HomeDtos.CarouselResponseDto>> GetAllCarouselsAsync(
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Listando carousels da home.");

        try
        {
            var carousels = await _homeRepository.GetAllCarouselsAsync(ct).ConfigureAwait(false);
            return [.. carousels.Select(MapToCarouselResponse)];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar carousels da home.");
            throw;
        }
    }

    public async Task<HomeDtos.CarouselResponseDto> GetCarouselByIdAsync(
        Guid carouselId,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Obtendo carousel da home {CarouselId}.", carouselId);

        try
        {
            var carousel =
                await _homeRepository.GetCarouselByIdAsync(carouselId, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("Carousel", carouselId);
            return MapToCarouselResponse(carousel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter carousel da home {CarouselId}.", carouselId);
            throw;
        }
    }

    public async Task<HomeDtos.CarouselResponseDto> CreateCarouselAsync(
        HomeDtos.CarouselCreateDto request,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Criando carousel da home. Request: {@Request}", request);

        try
        {
            ArgumentNullException.ThrowIfNull(request);

            var created = await _homeRepository
                .CreateCarouselAsync(MapToCarouselModel(request), ct)
                .ConfigureAwait(false);

            // Reload to get the Included Upload for mapping
            created = await _homeRepository.GetCarouselByIdAsync(created.Id, ct).ConfigureAwait(false)
                ?? created;

            _logger.LogInformation("Carousel created: {CarouselId}", created.Id);
            return MapToCarouselResponse(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar carousel da home. Request: {@Request}", request);
            throw;
        }
    }

    public async Task<HomeDtos.CarouselResponseDto> UpdateCarouselAsync(
        Guid id,
        HomeDtos.CarouselUpdateDto req,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation(
            "Atualizando carousel da home {CarouselId}. Request: {@Request}",
            id,
            req
        );

        try
        {
            ArgumentNullException.ThrowIfNull(req);
            var c =
                await _homeRepository.GetCarouselByIdAsync(id, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("Carousel", id);

            c.Order = req.Order ?? c.Order;
            if (req.Section.HasValue)
                c.Section = MapEnum<HomeDomain.CarouselSection>(req.Section.Value);
            if (req.Title is not null)
                c.Title = Normalize(req.Title, nameof(req.Title));
            if (req.UploadId.HasValue)
                c.UploadId = req.UploadId.Value;
            c.Subtitle = req.Subtitle is not null ? req.Subtitle.Trim() : c.Subtitle;
            c.ActionUrl = req.ActionUrl is not null ? req.ActionUrl.Trim() : c.ActionUrl;

            var updated = await _homeRepository.UpdateCarouselAsync(c, ct).ConfigureAwait(false);

            // Reload to get the Included Upload for mapping
            updated = await _homeRepository.GetCarouselByIdAsync(updated.Id, ct).ConfigureAwait(false)
                ?? updated;

            return MapToCarouselResponse(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao atualizar carousel da home {CarouselId}. Request: {@Request}",
                id,
                req
            );
            throw;
        }
    }

    public async Task DeleteCarouselAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Excluindo carousel da home {CarouselId}.", id);

        try
        {
            var c =
                await _homeRepository.GetCarouselByIdAsync(id, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("Carousel", id);
            await _homeRepository.DeleteCarouselAsync(c, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir carousel da home {CarouselId}.", id);
            throw;
        }
    }

    #endregion

    #region StatCard Operations

    public async Task<IReadOnlyList<HomeDtos.StatCardResponseDto>> GetAllStatCardsAsync(
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Listando stat cards da home.");

        try
        {
            var stats = await _homeRepository.GetAllStatCardsAsync(ct).ConfigureAwait(false);
            return [.. stats.Select(MapToStatCardResponse)];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar stat cards da home.");
            throw;
        }
    }

    public async Task<HomeDtos.StatCardResponseDto> GetStatCardByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Obtendo stat card da home {StatCardId}.", id);

        try
        {
            var stat =
                await _homeRepository.GetStatCardByIdAsync(id, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("StatCard", id);
            return MapToStatCardResponse(stat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter stat card da home {StatCardId}.", id);
            throw;
        }
    }

    public async Task<HomeDtos.StatCardResponseDto> CreateStatCardAsync(
        HomeDtos.StatCardCreateDto request,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Criando stat card da home. Request: {@Request}", request);

        try
        {
            ArgumentNullException.ThrowIfNull(request);

            var created = await _homeRepository
                .CreateStatCardAsync(MapToStatCardModel(request), ct)
                .ConfigureAwait(false);

            return MapToStatCardResponse(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar stat card da home. Request: {@Request}", request);
            throw;
        }
    }

    public async Task<HomeDtos.StatCardResponseDto> UpdateStatCardAsync(
        Guid id,
        HomeDtos.StatCardUpdateDto req,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation(
            "Atualizando stat card da home {StatCardId}. Request: {@Request}",
            id,
            req
        );

        try
        {
            ArgumentNullException.ThrowIfNull(req);
            var s =
                await _homeRepository.GetStatCardByIdAsync(id, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("StatCard", id);

            s.Order = req.Order ?? s.Order;
            if (req.Color.HasValue)
                s.Color = MapEnum<HomeDomain.StatCardColor>(req.Color.Value);
            if (req.Title is not null)
                s.Title = Normalize(req.Title, nameof(req.Title));
            if (req.Value is not null)
                s.Value = Normalize(req.Value, nameof(req.Value));
            if (req.Description is not null)
                s.Description = Normalize(req.Description, nameof(req.Description));
            if (req.IconName is not null)
                s.IconName = Normalize(req.IconName, nameof(req.IconName));

            var updated = await _homeRepository.UpdateStatCardAsync(s, ct).ConfigureAwait(false);
            return MapToStatCardResponse(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao atualizar stat card da home {StatCardId}. Request: {@Request}",
                id,
                req
            );
            throw;
        }
    }

    public async Task DeleteStatCardAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Excluindo stat card da home {StatCardId}.", id);

        try
        {
            var s =
                await _homeRepository.GetStatCardByIdAsync(id, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("StatCard", id);
            await _homeRepository.DeleteStatCardAsync(s, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir stat card da home {StatCardId}.", id);
            throw;
        }
    }

    #endregion

    #region Helpers & Mappings

    private HomeDtos.CarouselResponseDto MapToCarouselResponse(HomeDomain.CarouselModel c) =>
        new(
            c.Id,
            c.Title,
            c.Subtitle,
            BuildPublicUploadUrl(c.Upload),
            c.ActionUrl,
            c.Order,
            MapEnum<HomeDtos.CarouselSection>(c.Section)
        );

    private static HomeDomain.CarouselModel MapToCarouselModel(HomeDtos.CarouselCreateDto r) =>
        new()
        {
            Title = Normalize(r.Title, nameof(r.Title)),
            Subtitle = r.Subtitle?.Trim(),
            UploadId = r.UploadId,
            ActionUrl = r.ActionUrl?.Trim(),
            Order = r.Order,
            Section = MapEnum<HomeDomain.CarouselSection>(r.Section),
        };

    private static HomeDtos.StatCardResponseDto MapToStatCardResponse(HomeDomain.StatCardModel s) =>
        new(
            s.Id,
            s.Title,
            s.Value,
            s.Description,
            s.IconName,
            MapEnum<HomeDtos.StatCardColor>(s.Color),
            s.Order
        );

    private static HomeDomain.StatCardModel MapToStatCardModel(HomeDtos.StatCardCreateDto r) =>
        new()
        {
            Title = Normalize(r.Title, nameof(r.Title)),
            Value = Normalize(r.Value, nameof(r.Value)),
            Description = Normalize(r.Description, nameof(r.Description)),
            IconName = Normalize(r.IconName, nameof(r.IconName)),
            Color = MapEnum<HomeDomain.StatCardColor>(r.Color),
            Order = r.Order,
        };

    private static T MapEnum<T>(object source)
        where T : struct, Enum => Enum.Parse<T>(source.ToString()!, false);

    private static string Normalize(string value, string param) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw LogicException.InvalidValue(param, value);

    private string BuildPublicUploadUrl(UploadModel? upload)
    {
        if (upload is null || string.IsNullOrWhiteSpace(upload.StoragePath))
        {
            return string.Empty;
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var fileName = Path.GetFileName(upload.StoragePath);
        var relativePath = $"/uploads/{fileName}";

        if (request is null)
        {
            return relativePath;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}{relativePath}";
    }

    #endregion
}
