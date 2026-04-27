using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Drains.Application.DTOs;

public sealed record DrainCreateRequest(
    [Required, StringLength(255), Display(Name = "Nome do bueiro", Description = "Nome do bueiro")]
        string Name,
    [Required, StringLength(500), Display(Name = "Endereço do bueiro", Description = "Endereço do bueiro")]
        string Address,
    [Required, Range(-90, 90), Display(Name = "Latitude do bueiro", Description = "Latitude do bueiro")]
        double Latitude,
    [Required, Range(-180, 180), Display(Name = "Longitude do bueiro", Description = "Longitude do bueiro")]
        double Longitude,
    [Required, StringLength(100), Display(Name = "ID do hardware", Description = "ID único do hardware associado ao bueiro")]
    [property: JsonPropertyName("hardware_id")]
        string HardwareId,
    [property:
        JsonPropertyName("is_active"),
        Display(Name = "Bueiro ativo", Description = "Status de atividade do bueiro")
    ]
        bool IsActive = true
);

public sealed record DrainUpdateRequest(
    [StringLength(255), Display(Name = "Nome do bueiro", Description = "Nome do bueiro")]
        string? Name = null,
    [StringLength(500), Display(Name = "Endereço do bueiro", Description = "Endereço do bueiro")]
        string? Address = null,
    [Range(-90, 90), Display(Name = "Latitude do bueiro", Description = "Latitude do bueiro")]
        double? Latitude = null,
    [Range(-180, 180), Display(Name = "Longitude do bueiro", Description = "Longitude do bueiro")]
        double? Longitude = null,
    [property:
        JsonPropertyName("is_active"),
        Display(Name = "Bueiro ativo", Description = "Status de atividade do bueiro")
    ]
        bool? IsActive = null,
    [StringLength(100), Display(Name = "ID do hardware", Description = "ID único do hardware associado ao bueiro")]
    [property: JsonPropertyName("hardware_id")]
        string? HardwareId = null
);

public sealed record DrainResponse(
    Guid Id,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("hardware_id")] string HardwareId,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);
