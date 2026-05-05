using backend.Features.Feedbacks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Feedbacks.Infrastructure.Persistence.Configuration;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("feedbacks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Comment)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.IsEdited)
            .HasDefaultValue(false);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Índice para buscas por usuário e ordenação por data
        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.CreatedAt);
    }
}