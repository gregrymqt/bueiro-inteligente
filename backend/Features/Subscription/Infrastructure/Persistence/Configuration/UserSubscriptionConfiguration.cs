using backend.Features.Subscription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Subscription.Infrastructure.Configuration;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");
        builder.HasKey(x => x.Id);

        // Índice único para o ID externo do Mercado Pago
        builder.HasIndex(x => x.ExternalId).IsUnique();

        builder.Property(x => x.ExternalId).IsRequired().HasMaxLength(150);
        builder.Property(x => x.PayerEmail).IsRequired().HasMaxLength(200);
        
        // Salva o Enum como string no PostgreSQL via Npgsql
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}