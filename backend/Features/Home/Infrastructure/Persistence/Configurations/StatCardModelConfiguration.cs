using backend.Features.Home.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Home.Infrastructure.Persistence.Configurations;

public sealed class StatCardModelConfiguration : IEntityTypeConfiguration<StatCardModel>
{
    public void Configure(EntityTypeBuilder<StatCardModel> builder)
    {
        builder.ToTable("home_stats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();

        builder.Property(x => x.Value).HasColumnName("value").HasMaxLength(255).IsRequired();

        builder
            .Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.IconName).HasColumnName("icon_name").HasMaxLength(255).IsRequired();

        builder
            .Property(x => x.Color)
            .HasColumnName("color")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Order).HasColumnName("order").HasDefaultValue(0);
    }
}
