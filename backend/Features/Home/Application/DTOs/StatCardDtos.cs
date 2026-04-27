using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Home.Application.DTOs;

/// <summary>
/// Payload used to create a statistic card.
/// </summary>
public sealed record StatCardCreateDto(
    [Required, StringLength(255), Display(Name = "Título", Description = "Título do card")] string Title,
    [Required, StringLength(255), Display(Name = "Valor", Description = "Valor exibido no card")] string Value,
    [Required, StringLength(255), Display(Name = "Descrição", Description = "Descrição do card")] string Description,
    [Required, StringLength(255), Display(Name = "Nome do ícone", Description = "Nome do ícone do Lucide")]
    [property: JsonPropertyName("icon_name")]
        string IconName,
    [Required, Display(Name = "Cor", Description = "Cor visual do card")] StatCardColor Color,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do card")] int Order
);

/// <summary>
/// Payload used to update a statistic card.
/// </summary>
public sealed record StatCardUpdateDto(
    [StringLength(255), Display(Name = "Título", Description = "Título do card")] string? Title = null,
    [StringLength(255), Display(Name = "Valor", Description = "Valor exibido no card")] string? Value = null,
    [StringLength(255), Display(Name = "Descrição", Description = "Descrição do card")] string? Description = null,
    [StringLength(255), Display(Name = "Nome do ícone", Description = "Nome do ícone do Lucide")]
    [property: JsonPropertyName("icon_name")]
        string? IconName = null,
    [Display(Name = "Cor", Description = "Cor visual do card")] StatCardColor? Color = null,
    [Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do card")] int? Order = null
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