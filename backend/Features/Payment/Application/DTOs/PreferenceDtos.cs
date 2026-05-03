namespace backend.Features.Payment.Application.DTOs;

public record CreatePreferenceRequestDto(
    string Title,
    string Description,
    decimal UnitPrice,
    string PayerEmail,
    Guid? PlanId
);

public record PreferenceResponseDto(
    string PreferenceId,
    string InitPoint, // Link do Checkout Pro
    Guid ExternalReference
);
