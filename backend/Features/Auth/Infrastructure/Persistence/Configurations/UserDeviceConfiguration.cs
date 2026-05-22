using backend.Features.Auth.Domain.Entities;
using backend.Features.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Users.Infrastructure.Persistence.Configurations;

public sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        // 1. Nome da tabela mapeado para snake_case (Padrão recomendável para PostgreSQL)
        builder.ToTable("user_devices");

        // 2. Definição da Chave Primária
        builder.HasKey(d => d.Id);

        // 3. Configuração das Propriedades
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Informa que o Guid é gerado no C# pelo construtor da model

        builder.Property(d => d.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(d => d.FcmToken)
            .HasColumnName("fcm_token")
            .IsRequired()
            .HasMaxLength(500); // Tokens do FCM são strings longas. 500 caracteres cobrem com segurança.

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // 4. Criação de Índices de Performance (Crucial para o seu fluxo)
        
        // Índice único no FcmToken: evita que o mesmo aparelho seja cadastrado duas vezes
        builder.HasIndex(d => d.FcmToken)
            .IsUnique()
            .HasDatabaseName("ix_user_devices_fcm_token");

        // Índice no UserId: otimiza a consulta na hora que o backend buscar os aparelhos de um usuário específico para enviar o push
        builder.HasIndex(d => d.UserId)
            .HasDatabaseName("ix_user_devices_user_id");

    
          builder.HasOne<User>() // Substitua pelo nome da sua classe de Usuário se houver
          .WithMany()
          .HasForeignKey(d => d.UserId)
         .OnDelete(DeleteBehavior.Cascade); // Se o usuário for excluído, remove os tokens dele
         
    }
}