using System.Text.Json.Serialization;

namespace backend.Features.Home.Application.DTOs;

/// <summary>
/// Combined Home payload containing carousel items and statistic cards.
/// </summary>
public sealed record HomeResponseDto(
    [property: JsonPropertyName("carousels")] IReadOnlyList<CarouselResponseDto> Carousels,
    [property: JsonPropertyName("stats")] IReadOnlyList<StatCardResponseDto> Stats
);