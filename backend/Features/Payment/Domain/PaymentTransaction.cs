using System;

namespace backend.Features.Payment.Domain;

public class PaymentTransaction
{
    public Guid Id { get; private set; }

    // Relação com o usuário/cliente do Bueiro Inteligente
    public Guid UserId { get; private set; }

    // Opcional: Relacionamento com a assinatura/plano, caso seja o pagamento de um plano
    public Guid? PlanId { get; private set; }

    public decimal Amount { get; private set; }

    // Tipo de pagamento: "pix", "credit_card", "preference"
    public string PaymentMethodType { get; private set; } = string.Empty;

    // Status oficial do Mercado Pago (ex: pending, approved, rejected, cancelled)
    public string Status { get; private set; } = "pending";

    // Detalhe do status (ex: waiting_transfer, accredited, cc_rejected_bad_filled_date)
    public string? StatusDetail { get; private set; }

    // ==========================================
    // Identificadores do Mercado Pago
    // ==========================================

    // ID retornado em pagamentos diretos (Pix/Cartão)
    public long? MercadoPagoPaymentId { get; private set; }

    // ID retornado no endpoint /v1/orders (usado na geração do Pix)
    public string? MercadoPagoOrderId { get; private set; }

    // ID retornado ao gerar o link do Checkout Pro (Preferences)
    public string? MercadoPagoPreferenceId { get; private set; }

    // ==========================================
    // Dados Específicos: Pix e Boleto/Lotérica
    // ==========================================
    public string? PixQrCode { get; private set; } // Copia e Cola
    public string? PixQrCodeBase64 { get; private set; } // Imagem em Base64
    public string? TicketUrl { get; private set; } // Link externo com instruções
    public DateTimeOffset? ExpirationDate { get; private set; }

    // ==========================================
    // Dados Específicos: Cartão de Crédito
    // ==========================================
    public string? CardLastFourDigits { get; private set; }
    public int? Installments { get; private set; } // Número de parcelas

    // Auditoria
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Construtor vazio para o Entity Framework
    protected PaymentTransaction() { }

    // Construtor para inicialização via código
    public PaymentTransaction(
        Guid userId,
        decimal amount,
        string paymentMethodType,
        Guid? planId = null
    )
    {
        Id = Guid.NewGuid(); // Este ID será enviado como 'external_reference' para o MP
        UserId = userId;
        Amount = amount;
        PaymentMethodType = paymentMethodType;
        PlanId = planId;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = "pending";
    }

    // ==========================================
    // Métodos de Comportamento (Rich Domain Model)
    // ==========================================

    public void SetPixData(
        string orderId,
        string qrCode,
        string qrCodeBase64,
        string ticketUrl,
        DateTimeOffset expirationDate
    )
    {
        MercadoPagoOrderId = orderId;
        PixQrCode = qrCode;
        PixQrCodeBase64 = qrCodeBase64;
        TicketUrl = ticketUrl;
        ExpirationDate = expirationDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPreferenceData(string preferenceId, string initPointUrl)
    {
        MercadoPagoPreferenceId = preferenceId;
        TicketUrl = initPointUrl; // Reaproveitamos o TicketUrl para guardar o link de pagamento do Checkout Pro
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCreditCardData(long paymentId, string lastFourDigits, int installments)
    {
        MercadoPagoPaymentId = paymentId;
        CardLastFourDigits = lastFourDigits;
        Installments = installments;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(string status, string? statusDetail, long? paymentId = null)
    {
        Status = status;
        StatusDetail = statusDetail;

        if (paymentId.HasValue)
            MercadoPagoPaymentId = paymentId.Value;

        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
