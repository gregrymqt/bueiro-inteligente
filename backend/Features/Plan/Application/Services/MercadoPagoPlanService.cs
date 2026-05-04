using System.Text.Json;
using backend.Features.MercadoPago.Application.Base;
using backend.Features.Plan.Application.DTOs;
using backend.Features.Subscription.Application.DTOs;
using backend.Features.Subscription.Application.Interfaces;

namespace backend.Features.Subscription.Application.Services;

public class MercadoPagoPlanService(
    IHttpClientFactory httpClientFactory,
    ILogger<MercadoPagoPlanService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), IMercadoPagoPlanService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<MercadoPagoPlanResponse?> CreatePlanAsync(MercadoPagoPlanRequest request)
    {
        try
        {
            // Realiza o POST utilizando a classe base. O header de idempotência é gerado automaticamente.
            var jsonResponse = await SendMercadoPagoRequestAsync(
                HttpMethod.Post,
                "preapproval_plan",
                request
            );

            return JsonSerializer.Deserialize<MercadoPagoPlanResponse>(jsonResponse, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Falha ao criar o plano de assinatura no Mercado Pago.");
            return null;
        }
    }

    public async Task<MercadoPagoPlanResponse?> GetPlanAsync(string planId)
    {
        try
        {
            // Realiza o GET utilizando a classe base. Payload é nulo.
            var jsonResponse = await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Get,
                $"preapproval_plan/{planId}",
                null
            );

            return JsonSerializer.Deserialize<MercadoPagoPlanResponse>(jsonResponse, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Falha ao buscar detalhes do plano {PlanId} no Mercado Pago.", planId);
            return null;
        }
    }

    public async Task<MercadoPagoPlanResponse?> UpdatePlanAsync(string planId, MercadoPagoPlanRequest request)
    {
        try
        {
            // Realiza o PUT utilizando a classe base. O header de idempotência é gerado automaticamente.[cite: 18]
            var jsonResponse = await SendMercadoPagoRequestAsync(
                HttpMethod.Put,
                $"preapproval_plan/{planId}",
                request
            );

            return JsonSerializer.Deserialize<MercadoPagoPlanResponse>(jsonResponse, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Falha ao atualizar o plano {PlanId} no Mercado Pago.", planId);
            return null;
        }
    }
}