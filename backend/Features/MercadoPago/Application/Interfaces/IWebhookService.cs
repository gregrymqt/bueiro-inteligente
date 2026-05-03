using backend.Features.MercadoPago.Application.DTOs;

namespace backend.Features.MercadoPago.Application.Interfaces;

public interface IWebhookService
{
    bool IsSignatureValid(HttpRequest request, MercadoPagoWebhookNotification notification);
    Task ProcessWebhookNotificationAsync(MercadoPagoWebhookNotification notification);
}

// Interface que você implementará na sua camada de Infraestrutura para chamar o Hangfire
public interface IQueueService
{
    Task EnqueueJobAsync<TJob, TPayload>(TPayload payload)
        where TJob : class;
}
