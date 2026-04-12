using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Drains.Application.DTOs;

public sealed record DrainCreateRequest(
    [property:
        Required,
        StringLength(255),
        Display(Name = "Nome do bueiro", Description = "Nome do bueiro")
    ]
        string Name,
    [property:
        Required,
        StringLength(500),
        Display(Name = "Endereço do bueiro", Description = "Endereço do bueiro")
    ]
        string Address,
    [property:
        Required,
        Range(-90, 90),
        Display(Name = "Latitude do bueiro", Description = "Latitude do bueiro")
    ]
        double Latitude,
    [property:
        Required,
        Range(-180, 180),
        Display(Name = "Longitude do bueiro", Description = "Longitude do bueiro")
    ]
        double Longitude,
    [property:
        Required,
        StringLength(100),
        JsonPropertyName("hardware_id"),
        Display(Name = "ID do hardware", Description = "ID único do hardware associado ao bueiro")
    ]
        string HardwareId,
    [property:
        JsonPropertyName("is_active"),
        Display(Name = "Bueiro ativo", Description = "Status de atividade do bueiro")
    ]
        bool IsActive = true
);

public sealed record DrainUpdateRequest(
    [property: StringLength(255), Display(Name = "Nome do bueiro", Description = "Nome do bueiro")]
        string? Name = null,
    [property:
        StringLength(500),
        Display(Name = "Endereço do bueiro", Description = "Endereço do bueiro")
    ]
        string? Address = null,
    [property:
        Range(-90, 90),
        Display(Name = "Latitude do bueiro", Description = "Latitude do bueiro")
    ]
        double? Latitude = null,
    [property:
        Range(-180, 180),
        Display(Name = "Longitude do bueiro", Description = "Longitude do bueiro")
    ]
        double? Longitude = null,
    [property:
        JsonPropertyName("is_active"),
        Display(Name = "Bueiro ativo", Description = "Status de atividade do bueiro")
    ]
        bool? IsActive = null,
    [property:
        StringLength(100),
        JsonPropertyName("hardware_id"),
        Display(Name = "ID do hardware", Description = "ID único do hardware associado ao bueiro")
    ]
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
