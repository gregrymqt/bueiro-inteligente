using BueiroInteligente.Features.Monitoring.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BueiroInteligente.Features.Monitoring.Infrastructure.Persistence.Configurations;

public sealed class DrainStatusConfiguration : IEntityTypeConfiguration<DrainStatus>
{
    public void Configure(EntityTypeBuilder<DrainStatus> builder)
    {
        builder.ToTable("drain_status");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder
            .Property(x => x.DrainIdentifier)
            .HasColumnName("id_bueiro")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.DrainIdentifier);

        builder.Property(x => x.DistanceCm).HasColumnName("distancia_cm").IsRequired();

        builder.Property(x => x.ObstructionLevel).HasColumnName("nivel_obstrucao").IsRequired();

        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();

        builder.Property(x => x.Latitude).HasColumnName("latitude");

        builder.Property(x => x.Longitude).HasColumnName("longitude");

        builder
            .Property(x => x.LastUpdate)
            .HasColumnName("ultima_atualizacao")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder
            .Property(x => x.SyncedToRows)
            .HasColumnName("sincronizado_rows")
            .HasDefaultValue(false);
    }
}
