using backend.Features.MercadoPago.Application.DTOs;

namespace backend.Features.MercadoPago.Application.Interfaces;

public interface IWebhookService
{
    bool IsSignatureValid(HttpRequest request, MercadoPagoWebhookNotification notification);
    Task ProcessWebhookNotificationAsync(MercadoPagoWebhookNotification notification);
}
