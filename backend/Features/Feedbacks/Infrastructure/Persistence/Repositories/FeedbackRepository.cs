using backend.Features.Feedbacks.Application.DTOs;
using backend.Features.Feedbacks.Domain.Entities;
using backend.Features.Feedbacks.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Feedbacks.Infrastructure.Persistence.Repositories;

public class FeedbackRepository(AppDbContext dbContext) : IFeedbackRepository
{
    public async Task<IEnumerable<FeedbackResponseDTO>> GetAllAsync()
    {
        // Fazemos um Join ou Projeção para buscar os dados do usuário junto com o feedback
        return await dbContext.Feedbacks
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackResponseDTO(
                f.Id,
                "Lucas Vicente", // Aqui você buscaria: f.User.Name[cite: 21]
                "Desenvolvedor",  // Aqui você buscaria: f.User.Role[cite: 21]
                f.Comment,
                f.Rating,
                null,            // AvatarUrl (se houver no seu User)[cite: 21]
                f.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(Guid id)
    {
        return await dbContext.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task AddAsync(Feedback feedback)
    {
        await dbContext.Feedbacks.AddAsync(feedback);
        await dbContext.SaveChangesAsync(); // Persistência imediata conforme seu padrão
    }

    public async Task UpdateAsync(Feedback feedback)
    {
        dbContext.Feedbacks.Update(feedback);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        // Uso de ExecuteDeleteAsync do EF Core 8 para deletar sem carregar na memória
        await dbContext.Feedbacks
            .Where(f => f.Id == id)
            .ExecuteDeleteAsync();
    }
}