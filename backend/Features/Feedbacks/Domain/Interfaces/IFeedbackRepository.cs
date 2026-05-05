using backend.Features.Feedbacks.Application.DTOs;
using backend.Features.Feedbacks.Domain.Entities;

namespace backend.Features.Feedbacks.Domain.Interfaces;

public interface IFeedbackRepository
{
    Task<IEnumerable<FeedbackResponseDTO>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(Guid id);
    Task AddAsync(Feedback feedback);
    Task UpdateAsync(Feedback feedback);
    Task DeleteAsync(Guid id);
}