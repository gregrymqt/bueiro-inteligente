using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Home.Application.DTOs;

/// <summary>
/// Payload used to create a carousel item.
/// </summary>
public sealed record CarouselCreateDto(
    [property: Required, StringLength(255), Display(Name = "Título", Description = "Título do carousel")] string Title,
    [property: StringLength(255), Display(Name = "Subtítulo", Description = "Subtítulo do carousel")] string? Subtitle,
    [property: Required, Url, StringLength(2048), JsonPropertyName("image_url"), Display(Name = "URL da imagem", Description = "URL da imagem do carousel")] string ImageUrl,
    [property: Url, StringLength(2048), JsonPropertyName("action_url"), Display(Name = "URL de ação", Description = "URL opcional de ação do carousel")] string? ActionUrl,
    [property: Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do carousel")] int Order,
    [property: Required, Display(Name = "Seção", Description = "Seção do carousel na Home")] CarouselSection Section
);

/// <summary>
/// Payload used to update a carousel item.
/// </summary>
public sealed record CarouselUpdateDto(
    [property: StringLength(255), Display(Name = "Título", Description = "Título do carousel")] string? Title = null,
    [property: StringLength(255), Display(Name = "Subtítulo", Description = "Subtítulo do carousel")] string? Subtitle = null,
    [property: Url, StringLength(2048), JsonPropertyName("image_url"), Display(Name = "URL da imagem", Description = "URL da imagem do carousel")] string? ImageUrl = null,
    [property: Url, StringLength(2048), JsonPropertyName("action_url"), Display(Name = "URL de ação", Description = "URL opcional de ação do carousel")] string? ActionUrl = null,
    [property: Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do carousel")] int? Order = null,
    [property: Display(Name = "Seção", Description = "Seção do carousel na Home")] CarouselSection? Section = null
);

/// <summary>
/// Response used to expose a carousel item to API consumers.
/// </summary>
public sealed record CarouselResponseDto(
    Guid Id,
    string Title,
    string? Subtitle,
    [property: JsonPropertyName("image_url")] string ImageUrl,
    [property: JsonPropertyName("action_url")] string? ActionUrl,
    int Order,
    CarouselSection Section
);