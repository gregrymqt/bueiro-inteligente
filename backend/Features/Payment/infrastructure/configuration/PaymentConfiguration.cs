using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Entities; // Importa a model correta
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Payment.Infrastructure.Configuration
{
    public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            // Opcional, mas boa prática: definir o nome da tabela explicitamente
            builder.ToTable("PaymentTransactions");

            builder.HasKey(p => p.Id);

            // Relacionamentos e dados básicos
            builder.Property(p => p.UserId).IsRequired();
            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.PaymentMethodType).HasMaxLength(50).IsRequired();

            // Status do pagamento
            builder.Property(p => p.Status).HasMaxLength(50).IsRequired();
            builder.Property(p => p.StatusDetail).HasMaxLength(100);

            // Identificadores do Mercado Pago
            builder.Property(p => p.MercadoPagoOrderId).HasMaxLength(100);
            builder.Property(p => p.MercadoPagoPreferenceId).HasMaxLength(100);

            // Configurações específicas de Pix (Tamanho ajustado para suportar links grandes)
            builder.Property(p => p.PixQrCode).HasMaxLength(1000); // QR Code "Copia e Cola" pode ser extenso
            builder.Property(p => p.PixQrCodeBase64).HasColumnType("text"); // Mantido correto para Base64 longo
            builder.Property(p => p.TicketUrl).HasMaxLength(1000); // Ticket URL também pode ser extenso

            // CORREÇÃO: DateTimeOffset não usa MaxLength
            builder.Property(p => p.ExpirationDate).HasColumnType("timestamp with time zone");

            // Configurações específicas de Cartão de Crédito
            builder.Property(p => p.CardLastFourDigits).HasMaxLength(4);
            // CORREÇÃO: Removido o HasMaxLength(2) de Installments, pois é um tipo inteiro.

            // Auditoria
            builder
                .Property(p => p.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            builder.Property(p => p.UpdatedAt).HasColumnType("timestamp with time zone");
        }
    }
}
