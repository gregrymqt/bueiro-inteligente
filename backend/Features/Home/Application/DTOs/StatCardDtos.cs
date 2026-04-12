using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Home.Application.DTOs;

/// <summary>
/// Payload used to create a statistic card.
/// </summary>
public sealed record StatCardCreateDto(
    [property: Required, StringLength(255), Display(Name = "Título", Description = "Título do card")] string Title,
    [property: Required, StringLength(255), Display(Name = "Valor", Description = "Valor exibido no card")] string Value,
    [property: Required, StringLength(255), Display(Name = "Descrição", Description = "Descrição do card")] string Description,
    [property: Required, StringLength(255), JsonPropertyName("icon_name"), Display(Name = "Nome do ícone", Description = "Nome do ícone do Lucide")] string IconName,
    [property: Required, Display(Name = "Cor", Description = "Cor visual do card")] StatCardColor Color,
    [property: Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do card")] int Order
);

/// <summary>
/// Payload used to update a statistic card.
/// </summary>
public sealed record StatCardUpdateDto(
    [property: StringLength(255), Display(Name = "Título", Description = "Título do card")] string? Title = null,
    [property: StringLength(255), Display(Name = "Valor", Description = "Valor exibido no card")] string? Value = null,
    [property: StringLength(255), Display(Name = "Descrição", Description = "Descrição do card")] string? Description = null,
    [property: StringLength(255), JsonPropertyName("icon_name"), Display(Name = "Nome do ícone", Description = "Nome do ícone do Lucide")] string? IconName = null,
    [property: Display(Name = "Cor", Description = "Cor visual do card")] StatCardColor? Color = null,
    [property: Range(0, int.MaxValue), Display(Name = "Ordem", Description = "Ordem de exibição do card")] int? Order = null
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