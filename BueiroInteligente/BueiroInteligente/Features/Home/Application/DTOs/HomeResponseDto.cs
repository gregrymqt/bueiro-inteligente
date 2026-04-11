namespace BueiroInteligente.Features.Home.Application.DTOs;

/// <summary>
/// Combined Home payload containing carousel items and statistic cards.
/// </summary>
public sealed record HomeResponseDto(
    IReadOnlyList<CarouselResponseDto> Carousels,
    IReadOnlyList<StatCardResponseDto> Stats
);