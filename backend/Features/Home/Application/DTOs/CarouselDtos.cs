using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Home.Application.DTOs;

/// <summary>
/// Payload used to create a carousel item.
/// </summary>
public sealed record CarouselCreateDto(
    [Required, StringLength(255), Display(Name = "Título", Description = "Título do carousel")] string Title,
    [StringLength(255), Display(Name = "Subtítulo", Description = "Subtítulo do carousel")] string? Subtitle,
    [Required, Display(Name = "Upload Id", Description = "ID do upload da imagem do carousel")]
    [property: JsonPropertyName("upload_id")]
        Guid UploadId,
    [Url, StringLength(2048), Display(Name = "URL de ação", Description = "URL opcional de ação do carousel")]
    [property: JsonPropertyName("action_url")]
        string? ActionUrl,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do carousel")] int Order,
    [Required, Display(Name = "Seção", Description = "Seção do carousel na Home")] CarouselSection Section
);

/// <summary>
/// Payload used to update a carousel item.
/// </summary>
public sealed record CarouselUpdateDto(
    [StringLength(255), Display(Name = "Título", Description = "Título do carousel")] string? Title = null,
    [StringLength(255), Display(Name = "Subtítulo", Description = "Subtítulo do carousel")] string? Subtitle = null,
    [Display(Name = "Upload Id", Description = "ID do upload da imagem do carousel")]
    [property: JsonPropertyName("upload_id")]
        Guid? UploadId = null,
    [Url, StringLength(2048), Display(Name = "URL de ação", Description = "URL opcional de ação do carousel")]
    [property: JsonPropertyName("action_url")]
        string? ActionUrl = null,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do carousel")] int? Order = null,
    [Display(Name = "Seção", Description = "Seção do carousel na Home")] CarouselSection? Section = null
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