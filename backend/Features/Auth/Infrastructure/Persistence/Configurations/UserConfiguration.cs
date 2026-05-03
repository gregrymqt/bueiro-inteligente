using backend.Features.Auth.Domain;
using backend.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Auth.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();

        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(255);

        builder.Property(x => x.GoogleId).HasColumnName("google_id").HasMaxLength(255);

        builder.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(2048);

        builder
            .Property(x => x.EmailConfirmed)
            .HasColumnName("email_confirmed")
            .IsRequired()
            .HasDefaultValue(false);

        builder
            .Property(x => x.HashedPassword)
            .HasColumnName("hashed_password")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.GoogleId).IsUnique();

        builder
            .HasMany(x => x.Roles)
            .WithMany(x => x.Users)
            .UsingEntity<Dictionary<string, object>>(
                "user_roles",
                right =>
                    right
                        .HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("role_id")
                        .OnDelete(DeleteBehavior.Cascade),
                left =>
                    left
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("user_roles");
                    join.HasKey("user_id", "role_id");
                    join.Property<Guid>("user_id").HasColumnName("user_id").HasColumnType("uuid");
                    join.Property<Guid>("role_id").HasColumnName("role_id").HasColumnType("uuid");
                    join.HasIndex("role_id");
                }
            );
    }
}
