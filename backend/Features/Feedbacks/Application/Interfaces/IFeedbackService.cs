using backend.Features.Feedbacks.Application.DTOs;

namespace backend.Features.Feedbacks.Application.Interfaces;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackResponseDTO>> GetFeedbacksAsync();
    Task<FeedbackResponseDTO> SubmitFeedbackAsync(Guid userId, FeedbackCreateRequestDTO dto);
    Task<FeedbackResponseDTO> UpdateFeedbackAsync(Guid id, Guid userId, FeedbackUpdateRequestDTO dto);
    Task DeleteFeedbackAsync(Guid id, Guid userId);
}