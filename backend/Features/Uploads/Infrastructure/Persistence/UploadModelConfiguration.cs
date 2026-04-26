using backend.Features.Uploads.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Uploads.Infrastructure.Persistence;

public class UploadModelConfiguration : IEntityTypeConfiguration<UploadModel>
{
    public void Configure(EntityTypeBuilder<UploadModel> builder)
    {
        builder.ToTable("Uploads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StoragePath).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.Extension).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Checksum).IsRequired().HasMaxLength(128);
    }
}
