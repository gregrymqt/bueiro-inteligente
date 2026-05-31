using backend.Features.Home.Domain;
using backend.Features.Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Home.Infrastructure.Persistence.Configurations;

public sealed class CarouselModelConfiguration : IEntityTypeConfiguration<CarouselModel>
{
    public void Configure(EntityTypeBuilder<CarouselModel> builder)
    {
        builder.ToTable("home_carousels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();

        builder.Property(x => x.Subtitle).HasColumnName("subtitle").HasMaxLength(255);

        builder.Property(x => x.DesktopUploadId).HasColumnName("desktop_upload_id").HasColumnType("uuid").IsRequired();

        builder.HasOne(x => x.DesktopUpload)
            .WithMany()
            .HasForeignKey(x => x.DesktopUploadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.MobileUploadId).HasColumnName("mobile_upload_id").HasColumnType("uuid").IsRequired();

        builder.HasOne(x => x.MobileUpload)
            .WithMany()
            .HasForeignKey(x => x.MobileUploadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ActionUrl).HasColumnName("action_url").HasMaxLength(2048);

        builder.Property(x => x.Order).HasColumnName("order").HasDefaultValue(0);

        builder
            .Property(x => x.Section)
            .HasColumnName("section")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }
}
