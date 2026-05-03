using System.Security.Cryptography;
using System.Text;
using backend.core.Settings;
using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.MercadoPago.Application.Interfaces;
using backend.Features.Scheduler.Application.Jobs.MercadoPago;
using Microsoft.Extensions.Options;

namespace backend.Features.MercadoPago.Application.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly IQueueService _queueService;
    private readonly MercadoPagoSettings _mercadoPagoSettings;

    public WebhookService(
        ILogger<WebhookService> logger,
        IQueueService queueService,
        IOptions<MercadoPagoSettings> mercadoPagoSettings
    )
    {
        _logger = logger;
        _queueService = queueService;
        _mercadoPagoSettings = mercadoPagoSettings.Value;
    }

    public bool IsSignatureValid(HttpRequest request, MercadoPagoWebhookNotification notification)
    {
        // Alterado de WebhookSecret para WebhookKey para respeitar seu MercadoPagoSettings
        if (string.IsNullOrEmpty(_mercadoPagoSettings.WebhookKey))
        {
            _logger.LogWarning("WebhookKey não configurado. Validação ignorada.");
            return false;
        }

        try
        {
            if (
                !request.Headers.TryGetValue("x-request-id", out var xRequestId)
                || !request.Headers.TryGetValue("x-signature", out var xSignature)
            )
            {
                _logger.LogWarning("Headers de assinatura do Mercado Pago ausentes.");
                return false;
            }

            var signatureParts = xSignature.ToString().Split(',');
            var ts = signatureParts.FirstOrDefault(p => p.Trim().StartsWith("ts="))?.Split('=')[1];
            var hash = signatureParts
                .FirstOrDefault(p => p.Trim().StartsWith("v1="))
                ?.Split('=')[1];

            if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("Falha ao extrair 'ts' ou 'v1' da assinatura.");
                return false;
            }

            if (string.IsNullOrEmpty(notification.Data?.Id))
            {
                _logger.LogWarning("Payload sem Data.Id para validação.");
                return false;
            }

            var dataId = notification.Data.Id;
            var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";

            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(_mercadoPagoSettings.WebhookKey)
            );
            var calculatedHash = BitConverter
                .ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest)))
                .Replace("-", "")
                .ToLower();

            if (calculatedHash.Equals(hash))
                return true;

            _logger.LogWarning(
                "Assinatura inválida. Recebido: {Hash}, Calculado: {Calc}",
                hash,
                calculatedHash
            );
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na validação da assinatura do Webhook.");
            return false;
        }
    }

    public async Task ProcessWebhookNotificationAsync(MercadoPagoWebhookNotification notification)
    {
        if (string.IsNullOrEmpty(notification.Data?.Id))
        {
            _logger.LogWarning("Notificação recebida sem dados válidos (Data nulo ou Id vazio).");
            return;
        }

        try
        {
            var entityId = notification.Data.Id;

            // Os Jobs (Ex: ProcessPaymentNotificationJob) devem ser criados na sua pasta Application/Jobs
            switch (notification.Type)
            {
                case "payment":
                    var paymentData = new PaymentNotificationData { Id = entityId };
                    _logger.LogInformation(
                        "Enfileirando Job de Pagamento ID: {Id}",
                        paymentData.Id
                    );
                    await _queueService.EnqueueJobAsync<ProcessPaymentJob, PaymentNotificationData>(
                        paymentData
                    );
                    break;

                case "subscription_authorized_payment":
                    var subPaymentData = new PaymentNotificationData { Id = entityId };
                    _logger.LogInformation(
                        "Enfileirando Job de Pagamento de Assinatura ID: {Id}",
                        subPaymentData.Id
                    );
                    // await _queueService.EnqueueJobAsync<ProcessRenewalSubscriptionJob, PaymentNotificationData>(subPaymentData);
                    break;

                case "subscription_preapproval":
                    var paymenteData = new PaymentNotificationData { Id = entityId };
                    _logger.LogInformation(
                        "Enfileirando Job de Pré-aprovação de Assinatura ID: {Id}",
                        paymenteData.Id
                    );
                    // await _queueService.EnqueueJobAsync<ProcessPreapprovalSubscriptionJob, PaymentNotificationData>(paymenteData);
                    break;

                default:
                    _logger.LogWarning("Tipo '{Type}' não tratado.", notification.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar payload do webhook tipo {Type}",
                notification.Type
            );
        }
    }
}
