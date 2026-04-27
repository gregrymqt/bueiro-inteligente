using backend.Features.Uploads.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Uploads.Infrastructure.Persistence;

public class UploadModelConfiguration : IEntityTypeConfiguration<UploadModel>
{
    public void Configure(EntityTypeBuilder<UploadModel> builder)
    {
        builder.ToTable("uploads");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(255);
        builder.Property(x => x.ContentType).HasColumnName("content_type").IsRequired().HasMaxLength(100);
        builder.Property(x => x.Size).HasColumnName("size");
        builder.Property(x => x.StoragePath).HasColumnName("storage_path").IsRequired().HasMaxLength(1024);
        builder.Property(x => x.Extension).HasColumnName("extension").IsRequired().HasMaxLength(50);
        builder.Property(x => x.Checksum).HasColumnName("checksum").IsRequired().HasMaxLength(128);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

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