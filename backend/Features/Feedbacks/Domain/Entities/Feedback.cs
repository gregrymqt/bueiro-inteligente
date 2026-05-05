namespace backend.Features.Feedbacks.Domain.Entities;

public class Feedback
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Comment { get; private set; }
    public int Rating { get; private set; }
    public bool IsEdited { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Propriedade de Navegação (se você tiver a entidade User mapeada)
    // public virtual User User { get; private set; } 

    protected Feedback()
    {
    } // EF Core

    public Feedback(Guid userId, string comment, int rating)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Comment = comment;
        Rating = Math.Clamp(rating, 1, 5); // Garante entre 1 e 5 estrelas
        IsEdited = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string? comment, int? rating)
    {
        if (!string.IsNullOrWhiteSpace(comment) && comment != Comment)
        {
            Comment = comment;
            IsEdited = true;
        }

        if (rating.HasValue)
        {
            Rating = Math.Clamp(rating.Value, 1, 5);
            IsEdited = true;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}