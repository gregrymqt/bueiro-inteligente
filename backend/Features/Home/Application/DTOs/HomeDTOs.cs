using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Home.Application.DTOs;

/// <summary>
/// Payload used to create a carousel item.
/// </summary>
public sealed record CarouselCreateDto(
    [Required, StringLength(255), Display(Name = "Título", Description = "Título do carousel")]
        string Title,
    [StringLength(255), Display(Name = "Subtítulo", Description = "Subtítulo do carousel")]
        string? Subtitle,
    [Required, Display(Name = "Upload Id", Description = "ID do upload da imagem do carousel")]
    [property: JsonPropertyName("upload_id")]
        Guid UploadId,
    [
        Url,
        StringLength(2048),
        Display(Name = "URL de ação", Description = "URL opcional de ação do carousel")
    ]
    [property: JsonPropertyName("action_url")]
        string? ActionUrl,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do carousel")]
        int Order,
    [Required, Display(Name = "Seção", Description = "Seção do carousel na Home")]
        CarouselSection Section
);

/// <summary>
/// Payload used to update a carousel item.
/// </summary>
public sealed record CarouselUpdateDto(
    [StringLength(255), Display(Name = "Título", Description = "Título do carousel")]
        string? Title = null,
    [StringLength(255), Display(Name = "Subtítulo", Description = "Subtítulo do carousel")]
        string? Subtitle = null,
    [Display(Name = "Upload Id", Description = "ID do upload da imagem do carousel")]
    [property: JsonPropertyName("upload_id")]
        Guid? UploadId = null,
    [
        Url,
        StringLength(2048),
        Display(Name = "URL de ação", Description = "URL opcional de ação do carousel")
    ]
    [property: JsonPropertyName("action_url")]
        string? ActionUrl = null,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do carousel")]
        int? Order = null,
    [Display(Name = "Seção", Description = "Seção do carousel na Home")]
        CarouselSection? Section = null
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

/// <summary>
/// Combined Home payload containing carousel items and statistic cards.
/// </summary>
public sealed record HomeResponseDto(
    [property: JsonPropertyName("carousels")] IReadOnlyList<CarouselResponseDto> Carousels,
    [property: JsonPropertyName("stats")] IReadOnlyList<StatCardResponseDto> Stats
);

/// <summary>
/// Payload used to create a statistic card.
/// </summary>
public sealed record StatCardCreateDto(
    [Required, StringLength(255), Display(Name = "Título", Description = "Título do card")]
        string Title,
    [Required, StringLength(255), Display(Name = "Valor", Description = "Valor exibido no card")]
        string Value,
    [Required, StringLength(255), Display(Name = "Descrição", Description = "Descrição do card")]
        string Description,
    [
        Required,
        StringLength(255),
        Display(Name = "Nome do ícone", Description = "Nome do ícone do Lucide")
    ]
    [property: JsonPropertyName("icon_name")]
        string IconName,
    [Required, Display(Name = "Cor", Description = "Cor visual do card")] StatCardColor Color,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do card")]
        int Order
);

/// <summary>
/// Payload used to update a statistic card.
/// </summary>
public sealed record StatCardUpdateDto(
    [StringLength(255), Display(Name = "Título", Description = "Título do card")]
        string? Title = null,
    [StringLength(255), Display(Name = "Valor", Description = "Valor exibido no card")]
        string? Value = null,
    [StringLength(255), Display(Name = "Descrição", Description = "Descrição do card")]
        string? Description = null,
    [StringLength(255), Display(Name = "Nome do ícone", Description = "Nome do ícone do Lucide")]
    [property: JsonPropertyName("icon_name")]
        string? IconName = null,
    [Display(Name = "Cor", Description = "Cor visual do card")] StatCardColor? Color = null,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do card")]
        int? Order = null
);

/// <summary>
/// Response used to expose a statistic card to API consumers.
/// </summary>
public sealed record StatCardResponseDto(
    Guid Id,
    string Title,
    string Value,
    string Description,
    [property: JsonPropertyName("icon_name")] string IconName,
    StatCardColor Color,
    int Order
);
