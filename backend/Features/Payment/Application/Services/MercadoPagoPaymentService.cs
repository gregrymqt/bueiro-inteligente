using System.Text.Json;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces; // Assumindo que você crie a interface aqui
using backend.Features.Plan.Application.Base;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class MercadoPagoPaymentService(
    IHttpClientFactory httpClientFactory,
    ILogger<MercadoPagoPaymentService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), IMercadoPagoPaymentService
{
    public async Task<MpPaymentResponse?> GetPaymentAsync(string paymentId)
    {
        try
        {
            // O payload é nulo pois é um GET[cite: 18].
            var jsonResponse = await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Get,
                $"v1/payments/{paymentId}",
                null
            );

            // Desserializa a string JSON recebida para o nosso novo DTO
            var responseDto = JsonSerializer.Deserialize<MpPaymentResponse>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return responseDto;
        }
        catch (Exception ex)
        {
            // O MercadoPagoServiceBase já tratou falhas HTTP e de rede (ex: 404, 500)[cite: 18].
            // Este catch pega principalmente falhas de desserialização (JsonException).
            Logger.LogError(
                ex,
                "Falha ao processar e mapear os detalhes do Pagamento {PaymentId} do Mercado Pago.",
                paymentId
            );
            return null;
        }
    }
}
