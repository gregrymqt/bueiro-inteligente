// backend.Features.Subscription.Infrastructure.Persistence.Configurations.SubscriptionPlanConfiguration.cs
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

            // --- NOVOS CAMPOS ADICIONADOS ---

            builder.Property(x => x.IsPopular)
                .IsRequired()
                .HasDefaultValue(false); // Define o valor padrão direto no banco

            builder.Property(x => x.FeaturesJson)
                .IsRequired()
                // Em bancos relacionais como PostgreSQL ou SQL Server, 
                // VARCHAR(max) ou TEXT é o ideal para JSON. Se for limitado:
                .HasColumnType("text") 
                .HasDefaultValue("[]"); // Default para um array vazio em JSON
                
            // --------------------------------
        }
    }
}