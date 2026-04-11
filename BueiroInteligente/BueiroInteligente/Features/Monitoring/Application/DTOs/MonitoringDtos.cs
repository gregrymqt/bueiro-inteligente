using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BueiroInteligente.Features.Monitoring.Application.DTOs;

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
public sealed record DrainStatusDTO
{
    [JsonPropertyName("id_bueiro"), Required, StringLength(100)]
    public required string IdBueiro { get; init; }

    [JsonPropertyName("distancia_cm"), Required]
    public required double DistanciaCm { get; init; }

    [JsonPropertyName("nivel_obstrucao"), Required]
    public required double NivelObstrucao { get; init; }

    [JsonPropertyName("status"), Required, StringLength(32)]
    public required string Status { get; init; }

    [JsonPropertyName("latitude"), Range(-90, 90)]
    public double? Latitude { get; init; }

    [JsonPropertyName("longitude"), Range(-180, 180)]
    public double? Longitude { get; init; }

    [JsonPropertyName("ultima_atualizacao"), Required]
    public required DateTimeOffset UltimaAtualizacao { get; init; }
}