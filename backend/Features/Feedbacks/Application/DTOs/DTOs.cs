namespace backend.Features.Feedbacks.Application.DTOs;

public record FeedbackResponseDTO(
    Guid Id,
    string UserName,
    string Role,
    string Comment,
    int Rating,
    string? AvatarUrl,
    DateTime CreatedAt
);

public record FeedbackCreateRequestDTO(
    string Comment,
    int Rating
);

// Payload para atualização parcial conforme definido no seu index.ts[cite: 21]
public record FeedbackUpdateRequestDTO(
    string? Comment,
    int? Rating
);