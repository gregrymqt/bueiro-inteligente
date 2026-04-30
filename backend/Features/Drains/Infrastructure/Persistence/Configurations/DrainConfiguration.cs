using backend.Features.Drains.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Drains.Infrastructure.Persistence.Configurations;

public sealed class DrainConfiguration : IEntityTypeConfiguration<Drain>
{
    public void Configure(EntityTypeBuilder<Drain> builder)
    {
        builder.ToTable("drains");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("longitude").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(x => x.HardwareId).HasColumnName("hardware_id").HasMaxLength(100).IsRequired();

        // ⚠️ MUDANÇA AQUI: Trocamos o HasIndex por HasAlternateKey
        builder.HasAlternateKey(x => x.HardwareId);

        builder.Property(x => x.MaxHeight).HasColumnName("max_height").HasDefaultValue(120.0);
        builder.Property(x => x.CriticalThreshold).HasColumnName("critical_threshold").HasDefaultValue(80.0);
        builder.Property(x => x.AlertThreshold).HasColumnName("alert_threshold").HasDefaultValue(50.0);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");
    }
}