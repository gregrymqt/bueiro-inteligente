using System.Text.Json;
using backend.Features.MercadoPago.Application.Base;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class MercadoPagoOrderService(
    IHttpClientFactory httpClientFactory,
    ILogger<MercadoPagoOrderService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), IMercadoPagoOrderService
{
    public async Task<MpOrderResponse?> GetOrderAsync(string? orderId)
    {
        try
        {
            var jsonResponse = await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Get,
                $"v1/orders/{orderId}",
                null
            );

            return JsonSerializer.Deserialize<MpOrderResponse>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Falha ao processar e mapear a Order {OrderId} do Mercado Pago.", orderId);
            return null;
        }
    }

    public async Task<bool> UpdateTransactionAsync(string orderId, string transactionId,
        MpUpdateTransactionRequest request)
    {
        try
        {
            // O SendMercadoPagoRequestAsync automaticamente injetará o X-Idempotency-Key
            // já que estamos usando HttpMethod.Put!
            await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Put,
                $"v1/orders/{orderId}/transactions/{transactionId}",
                request
            );

            Logger.LogInformation("Transação {TransactionId} da Ordem {OrderId} atualizada com sucesso.", transactionId,
                orderId);
            return true;
        }
        catch (Exception ex)
        {
            // O BaseService já capturou o corpo do erro (HTTP 4xx/5xx).
            Logger.LogError(ex, "Falha ao atualizar a transação {TransactionId} da Order {OrderId}.", transactionId,
                orderId);
            return false;
        }
    }

    public async Task<bool> DeleteTransactionAsync(string orderId, string transactionId)
    {
        try
        {
            // Passamos 'object' e 'null' pois o DELETE não requer body[cite: 20, 22]
            await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Delete,
                $"v1/orders/{orderId}/transactions/{transactionId}",
                null
            );

            Logger.LogInformation("Transação {TransactionId} da Ordem {OrderId} cancelada/deletada com sucesso.",
                transactionId, orderId);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Falha ao deletar a transação {TransactionId} da Order {OrderId}.", transactionId,
                orderId);
            return false;
        }
    }

    public async Task<MpOrderResponse> CreateOrderAsync<TRequest>(TRequest request)
    {
        // Centralizamos o POST v1/orders aqui. A classe base cuida do Idempotency-Key.
        var jsonResponse = await SendMercadoPagoRequestAsync(
            HttpMethod.Post,
            "v1/orders",
            request
        );

        var responseDto = JsonSerializer.Deserialize<MpOrderResponse>(
            jsonResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return responseDto ??
               throw new Exception("Falha ao desserializar a resposta de criação de Ordem do Mercado Pago.");
    }
}