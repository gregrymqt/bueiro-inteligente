using System.Text.Json;
using backend.Features.MercadoPago.Application.Base;
using backend.Features.Subscription.Application.DTOs;
using backend.Features.Subscription.Application.Interfaces;

namespace backend.Features.Subscription.Application.Services;

public sealed class MercadoPagoSubscriptionService(
    IHttpClientFactory httpClientFactory,
    ILogger<MercadoPagoSubscriptionService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), IMercadoPagoSubscriptionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SubscriptionResponse> CreateSubscriptionAsync(CreateSubscriptionRequest request)
    {
        // O método SendMercadoPagoRequestAsync cuida do POST e do Idempotency-Key
        var jsonResponse = await SendMercadoPagoRequestAsync(HttpMethod.Post, "preapproval", request)
            .ConfigureAwait(false);

        var result = JsonSerializer.Deserialize<SubscriptionResponse>(jsonResponse, JsonOptions);
        return result ?? throw new Exception("Falha ao desserializar resposta de criação de assinatura do Mercado Pago.");
    }

    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(string externalId,
        UpdateSubscriptionRequest request)
    {
        var jsonResponse = await SendMercadoPagoRequestAsync(HttpMethod.Put, $"preapproval/{externalId}", request)
            .ConfigureAwait(false);

        var result = JsonSerializer.Deserialize<SubscriptionResponse>(jsonResponse, JsonOptions);
        return result ??
               throw new Exception($"Falha ao desserializar resposta de atualização da assinatura {externalId}.");
    }

    public async Task<SubscriptionResponse?> GetSubscriptionAsync(string externalId)
    {
        try
        {
            var jsonResponse =
                await SendMercadoPagoRequestAsync(HttpMethod.Get, $"preapproval/{externalId}", (object?)null)
                    .ConfigureAwait(false);
            return JsonSerializer.Deserialize<SubscriptionResponse>(jsonResponse, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Assinatura {ExternalId} não encontrada ou erro ao buscar no Mercado Pago.",
                externalId);
            return null;
        }
    }
}