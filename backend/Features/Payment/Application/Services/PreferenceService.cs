using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain.Entities;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class PreferenceService(
    IUnitOfWork unitOfWork,
    IPaymentRepository paymentRepository,
    ILogger<PreferenceService> logger
) : IPreferenceService
{
    public async Task<PreferenceResponseDto> CreatePreferenceAsync(
        CreatePreferenceRequestDto request,
        Guid userId
    )
    {
        logger.LogInformation(
            "Iniciando criação de Preferência (Checkout Pro) para o utilizador {UserId}.",
            userId
        );

        var response = await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();

            var paymentTransaction = new PaymentTransaction(
                userId: userId,
                amount: request.UnitPrice,
                paymentMethodType: "preference",
                planId: request.PlanId
            );

            await paymentRepository.AddAsync(paymentTransaction);

            var preferenceRequest = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = request.Title,
                        Description = request.Description,
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = request.UnitPrice,
                    },
                },
                Payer = new PreferencePayerRequest { Email = request.PayerEmail },
                ExternalReference = paymentTransaction.Id.ToString(),
                AutoReturn = "approved",
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://bueirointeligente.com/payment/success",
                    Failure = "https://bueirointeligente.com/payment/failure",
                    Pending = "https://bueirointeligente.com/payment/pending",
                },
                BinaryMode = true,
            };

            // 🛡️ NOVO CÓDIGO: Bloco Try-Catch para a criação da Preferência
            Preference? preference = null;
            try
            {
                var client = new PreferenceClient();
                preference = await client.CreateAsync(preferenceRequest);

                if (preference == null || string.IsNullOrEmpty(preference.Id))
                {
                    throw new InvalidOperationException("O Mercado Pago falhou ao gerar o ID da Preferência.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao gerar a Preferência no Mercado Pago. Marcando transação como rejeitada localmente.");

                paymentTransaction.UpdateStatus("rejected", "preference_creation_failed");

                return new PreferenceResponseDto(
                    PreferenceId: string.Empty,
                    InitPoint: string.Empty,
                    ExternalReference: paymentTransaction.Id
                );
            }

            paymentTransaction.SetPreferenceData(preference.Id, preference.InitPoint);

            return new PreferenceResponseDto(
                PreferenceId: preference.Id,
                InitPoint: preference.InitPoint,
                ExternalReference: paymentTransaction.Id
            );
        });

        logger.LogInformation(
            "Preferência gerada com sucesso. ID: {PrefId}",
            response.PreferenceId
        );

        return response;
    }
}