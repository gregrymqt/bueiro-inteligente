using System.Text.Json;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Plan.Application.Base;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public interface IMercadoPagoOrderService
{
    Task<MpOrderResponse?> GetOrderAsync(string orderId);
}

public class MercadoPagoOrderService(
    IHttpClientFactory httpClientFactory,
    ILogger<MercadoPagoOrderService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), IMercadoPagoOrderService
{
    public async Task<MpOrderResponse?> GetOrderAsync(string? orderId)
    {
        try
        {
            // O GET não possui payload, então passamos 'object' como tipo genérico e 'null' no payload.
            var jsonResponse = await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Get,
                $"v1/orders/{orderId}",
                null
            );

            // Desserializamos a string JSON para o nosso DTO atualizado[cite: 3, 5].
            var responseDto = JsonSerializer.Deserialize<MpOrderResponse>(
                jsonResponse,
                // Usamos PropertyNameCaseInsensitive por segurança extra na leitura[cite: 10]
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return responseDto;
        }
        catch (Exception ex)
        {
            // O BaseService já loga erros HTTP e falhas de rede.
            // Este try/catch protege contra falhas de desserialização JSON[cite: 3].
            Logger.LogError(
                ex,
                "Falha ao processar e mapear a Order {OrderId} do Mercado Pago.",
                orderId
            );
            return null;
        }
    }
}
