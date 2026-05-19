using System.Text.Json.Serialization;

namespace backend.Features.Payment.Application.DTOs;

public record CreatePreferenceRequestDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
    [property: JsonPropertyName("payerEmail")] string PayerEmail,
    [property: JsonPropertyName("planId")] Guid? PlanId
);

public record PreferenceResponseDto(
    [property: JsonPropertyName("preferenceId")] string PreferenceId,
    [property: JsonPropertyName("initPoint")] string InitPoint, 
    [property: JsonPropertyName("externalReference")] Guid ExternalReference
);