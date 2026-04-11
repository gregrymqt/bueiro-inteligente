using System.Text.Json.Serialization;

namespace BueiroInteligente.Features.Home.Application.DTOs;

/// <summary>
/// Carousel sections supported by the Home page.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CarouselSection
{
    hero,
    alerts,
    stats,
}

/// <summary>
/// Visual severity colors supported by stat cards.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatCardColor
{
    success,
    warning,
    danger,
}