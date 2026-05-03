using System;
using System.Net.Http.Headers;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Application.Services;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Payment.Infrastructure.Persistence;
using backend.Features.Plan.Application.Interfaces;
using backend.Features.Plan.Application.Services;
using BueiroInteligente.Core.Settings;
using MercadoPago.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace backend.Extensions.Services.MercadoPago;

public static class MercadoPagoServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteMercadoPago(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Resgata as configurações do .env mapeadas na etapa anterior
        var mercadoPagoSettings = configuration
            .GetSection("MercadoPago")
            .Get<MercadoPagoSettings>();

        if (mercadoPagoSettings is null || string.IsNullOrEmpty(mercadoPagoSettings.AccessToken))
        {
            throw new InvalidOperationException(
                "Configurações do Mercado Pago não encontradas ou o AccessToken está vazio."
            );
        }

        // 1. Configura o SDK Oficial do Mercado Pago
        MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

        // 2. Configura o HttpClient com as políticas do Polly
        services
            .AddHttpClient(
                "MercadoPagoClient",
                client =>
                {
                    client.BaseAddress = new Uri("https://api.mercadopago.com");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        mercadoPagoSettings.AccessToken
                    );

                    client.Timeout = TimeSpan.FromSeconds(30);
                }
            )
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine(
                            $"[Mercado Pago] Tentativa {retryAttempt} de 3 após {timespan.TotalSeconds}s. Erro: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}"
                        );
                    }
                )
            )
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Console.WriteLine(
                            $"[Mercado Pago] ⚠️ CIRCUIT BREAKER ABERTO! Pausando requisições por {duration.TotalSeconds}s devido a falhas consecutivas."
                        );
                    },
                    onReset: () =>
                    {
                        Console.WriteLine(
                            "[Mercado Pago] ✅ Circuit Breaker fechado. Requisições normalizadas."
                        );
                    }
                )
            );

        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPixService, PixService>();
        services.AddScoped<ICreditCardService, CreditCardService>();
        services.AddScoped<IPreferenceService, PreferenceService>();

        return services;
    }
}
