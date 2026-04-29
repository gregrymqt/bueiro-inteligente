using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Monitoring.Application.DTOs;

public sealed record SensorPayloadDTO(
    [property: JsonPropertyName("id_bueiro")] [Required] [StringLength(100)] string IdBueiro,
    [property: JsonPropertyName("distancia_cm")] [Required] double DistanciaCm,
    [property: JsonPropertyName("latitude")] [Range(-90, 90)] double? Latitude = null,
    [property: JsonPropertyName("longitude")] [Range(-180, 180)] double? Longitude = null,
    [property: JsonPropertyName("ultima_atualizacao")] DateTimeOffset? UltimaAtualizacao = null
);

public sealed record DrainStatusDTO(
    [property: JsonPropertyName("id_bueiro")] string IdBueiro,
    [property: JsonPropertyName("distancia_cm")] double DistanciaCm,
    [property: JsonPropertyName("nivel_obstrucao")] double NivelObstrucao,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("latitude")] double? Latitude = null,
    [property: JsonPropertyName("longitude")] double? Longitude = null,
    [property: JsonPropertyName("ultima_atualizacao")] DateTimeOffset UltimaAtualizacao = default,
    [property: JsonPropertyName("data_hash")] string DataHash = ""
);