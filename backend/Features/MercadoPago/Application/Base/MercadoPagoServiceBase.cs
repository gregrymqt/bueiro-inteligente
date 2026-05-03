using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

// using backend.Core.Exceptions; // Descomente e ajuste para o namespace correto das suas exceções personalizadas

namespace backend.Features.Plan.Application.Base;

public abstract class MercadoPagoServiceBase
{
    protected readonly ILogger Logger;
    protected readonly HttpClient HttpClient;

    // Adicionado JsonSerializerOptions para garantir que o envio siga o padrão camelCase do Mercado Pago
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    protected MercadoPagoServiceBase(
        IHttpClientFactory httpClientFactory,
        ILogger<MercadoPagoServiceBase> logger
    )
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        HttpClient =
            httpClientFactory.CreateClient("MercadoPagoClient")
            ?? throw new InvalidOperationException(
                "Falha ao criar o HttpClient 'MercadoPagoClient'. O serviço não está registrado."
            );

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected async Task<string> SendMercadoPagoRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        T? payload
    )
    {
        ArgumentNullException.ThrowIfNull(method);

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("O endpoint não pode ser vazio.", nameof(endpoint));

        if (HttpClient.BaseAddress == null)
            throw new InvalidOperationException(
                "O BaseAddress do HttpClient não está configurado."
            );

        var requestUri = new Uri(HttpClient.BaseAddress, endpoint);
        using var request = new HttpRequestMessage(method, requestUri);

        // Header de Idempotência obrigatório para mutações financeiras
        if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch)
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            request.Headers.Add("X-Idempotency-Key", idempotencyKey);
        }

        if (payload != null)
        {
            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            Logger.LogInformation(
                "Enviando requisição para MP. Método: {Method}, Endpoint: {Endpoint}",
                method.Method,
                endpoint
            );

            Logger.LogDebug("Payload MP: {Payload}", jsonPayload);
        }

        try
        {
            var response = await HttpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return content;
            }

            Logger.LogError(
                "Erro na API do Mercado Pago. Status: {StatusCode}. Resposta: {ErrorContent}",
                response.StatusCode,
                content
            );

            var errorMessage =
                $"Erro na API do Mercado Pago. Status: {response.StatusCode}. Detalhes: {content}";

            // Substitua 'ExternalApiException' pela sua classe base de exceção de domínio, se necessário
            throw new Exception(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(
                ex,
                "Erro de rede ao comunicar com o Mercado Pago. Status: {StatusCode}",
                ex.StatusCode
            );
            throw; // Repassa a exceção para ser tratada globalmente pelo middleware
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            Logger.LogError(
                ex,
                "Erro inesperado ao processar requisição do Mercado Pago para o endpoint {Endpoint}.",
                endpoint
            );
            throw;
        }
    }
}
