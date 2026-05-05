using backend.Features.Feedbacks.Application.DTOs;
using backend.Features.Feedbacks.Application.Interfaces;
using backend.Features.Feedbacks.Domain.Entities;
using backend.Features.Feedbacks.Domain.Interfaces;
using backend.Features.Notifications.Application.DTOs;
using backend.Features.Notifications.Application.Interfaces;
using backend.Infrastructure.Persistence;

namespace backend.Features.Feedbacks.Application.Services;

public class FeedbackService(
    IFeedbackRepository repository,
    INotificationService notificationService, // Injetando a feature genérica
    AppDbContext dbContext
) : IFeedbackService
{
    public async Task<IEnumerable<FeedbackResponseDTO>> GetFeedbacksAsync()
    {
        // Aqui você pode incluir o Join com a tabela de Users para pegar UserName e Avatar
        return await repository.GetAllAsync();
    }

    public async Task<FeedbackResponseDTO> SubmitFeedbackAsync(Guid userId, FeedbackCreateRequestDTO dto)
    {
        var feedback = new Feedback(userId, dto.Comment, dto.Rating);

        await repository.AddAsync(feedback);

        // DISPARO DA NOTIFICAÇÃO: Feedback como componente utiliza o serviço de notificação
        await notificationService.SendNotificationAsync(
            userId,
            "Feedback Recebido! ⭐",
            "Obrigado por nos ajudar a melhorar o Bueiro Inteligente.",
            NotificationType.Success
        );

        return MapToResponse(feedback); // Método auxiliar de mapeamento
    }

    public async Task<FeedbackResponseDTO> UpdateFeedbackAsync(Guid id, Guid userId, FeedbackUpdateRequestDTO dto)
    {
        var feedback = await repository.GetByIdAsync(id)
                       ?? throw new Exception("Feedback não encontrado.");

        if (feedback.UserId != userId)
            throw new UnauthorizedAccessException("Você não pode editar este feedback.");

        feedback.Update(dto.Comment, dto.Rating);
        await repository.UpdateAsync(feedback);

        return MapToResponse(feedback);
    }

    public async Task DeleteFeedbackAsync(Guid id, Guid userId)
    {
        var feedback = await repository.GetByIdAsync(id);
        if (feedback != null && feedback.UserId == userId)
        {
            await repository.DeleteAsync(id);
        }
    }

    private FeedbackResponseDTO MapToResponse(Feedback f) =>
        new(f.Id, "Usuário", "Cliente", f.Comment, f.Rating, null, f.CreatedAt);
}