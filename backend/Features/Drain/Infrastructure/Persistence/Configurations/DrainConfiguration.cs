using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DrainEntity = global::backend.Features.Drain.Domain.Drain;

namespace backend.Features.Drain.Infrastructure.Persistence.Configurations;

public sealed class DrainConfiguration : IEntityTypeConfiguration<DrainEntity>
{
    public void Configure(EntityTypeBuilder<DrainEntity> builder)
    {
        builder.ToTable("drains");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();

        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500).IsRequired();

        builder.Property(x => x.Latitude).HasColumnName("latitude").IsRequired();

        builder.Property(x => x.Longitude).HasColumnName("longitude").IsRequired();

        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder
            .Property(x => x.HardwareId)
            .HasColumnName("hardware_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.HardwareId).IsUnique();

        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");
    }
}
