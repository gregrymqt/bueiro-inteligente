using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Monitoring.Application.DTOs;

/// <summary>
/// Payload sent by the hardware sensor.
/// </summary>
public sealed record SensorPayloadDTO(
    [property: JsonPropertyName("id_bueiro"), Required, StringLength(100)] string IdBueiro,
    [property: JsonPropertyName("distancia_cm"), Required] double DistanciaCm,
    [property: JsonPropertyName("latitude"), Range(-90, 90)] double? Latitude = null,
    [property: JsonPropertyName("longitude"), Range(-180, 180)] double? Longitude = null
);

/// <summary>
/// Monitoring status exposed to dashboards and external consumers.
/// </summary>
public sealed record DrainStatusDTO(
    [property: JsonPropertyName("id_bueiro"), Required, StringLength(100)] string IdBueiro,
    [property: JsonPropertyName("distancia_cm"), Required] double DistanciaCm,
    [property: JsonPropertyName("nivel_obstrucao"), Required] double NivelObstrucao,
    [property: JsonPropertyName("status"), Required, StringLength(32)] string Status,
    [property: JsonPropertyName("latitude"), Range(-90, 90)] double? Latitude = null,
    [property: JsonPropertyName("longitude"), Range(-180, 180)] double? Longitude = null,
    [property: JsonPropertyName("ultima_atualizacao"), Required]
        DateTimeOffset UltimaAtualizacao = default
);
