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

        builder.HasData(
            new UploadModel
            {
                Id = System.Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FileName = "sample_home_photo.jpg",
                ContentType = "image/jpeg",
                Size = 102400,
                StoragePath = "/var/www/uploads/11111111-1111-1111-1111-111111111111.jpg",
                Extension = ".jpg",
                Checksum = "A1B2C3D4E5F6",
                CreatedAt = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            }
        );
    }
}
