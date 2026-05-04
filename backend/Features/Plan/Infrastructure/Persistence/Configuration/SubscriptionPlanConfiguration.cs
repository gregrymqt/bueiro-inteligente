using backend.Features.Subscription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Subscription.Infrastructure.Persistence.Configurations
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("SubscriptionPlans");

            builder.HasKey(x => x.Id);

            // Índice para buscas rápidas vindas de Webhooks ou consultas por ID do MP
            builder.HasIndex(x => x.ExternalId).IsUnique();

            builder.Property(x => x.ExternalId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasMaxLength(20);

            builder.Property(x => x.FrequencyType)
                .HasMaxLength(20);
        }
    }
}