using backend.Features.Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Features.Payment.Infrastructure.Configuration
{
    public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            // Padronização do nome da tabela em snake_case minúsculo
            builder.ToTable("payment_transactions");

            builder.HasKey(p => p.Id);
            builder
                .Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .ValueGeneratedNever();

            // Dados básicos e relacionamentos
            builder
                .Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();
            builder.Property(p => p.PlanId).HasColumnName("plan_id").HasColumnType("uuid");
            builder
                .Property(p => p.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            builder
                .Property(p => p.PaymentMethodType)
                .HasColumnName("payment_method_type")
                .HasMaxLength(50)
                .IsRequired();

            // Status do pagamento
            builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            builder.Property(p => p.StatusDetail).HasColumnName("status_detail").HasMaxLength(100);

            // Identificadores do Mercado Pago (Inclusão do campo esquecido)
            builder
                .Property(p => p.MercadoPagoPaymentId)
                .HasColumnName("mercado_pago_payment_id")
                .HasMaxLength(100);
            builder
                .Property(p => p.MercadoPagoOrderId)
                .HasColumnName("mercado_pago_order_id")
                .HasMaxLength(100);
            builder
                .Property(p => p.MercadoPagoPreferenceId)
                .HasColumnName("mercado_pago_preference_id")
                .HasMaxLength(100);

            // Configurações específicas de Pix
            builder.Property(p => p.PixQrCode).HasColumnName("pix_qr_code").HasMaxLength(1000);
            builder
                .Property(p => p.PixQrCodeBase64)
                .HasColumnName("pix_qr_code_base64")
                .HasColumnType("text");
            builder.Property(p => p.TicketUrl).HasColumnName("ticket_url").HasMaxLength(1000);
            builder
                .Property(p => p.ExpirationDate)
                .HasColumnName("expiration_date")
                .HasColumnType("timestamp with time zone");

            // Configurações específicas de Cartão de Crédito
            builder
                .Property(p => p.CardLastFourDigits)
                .HasColumnName("card_last_four_digits")
                .HasMaxLength(4);
            builder.Property(p => p.Installments).HasColumnName("installments");

            // Auditoria
            builder
                .Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            builder
                .Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");
        }
    }
}
