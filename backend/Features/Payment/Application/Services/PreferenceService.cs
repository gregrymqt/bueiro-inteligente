using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Persistence; // Ajuste para o seu AppDbContext
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class PreferenceService(
    AppDbContext dbContext,
    IPaymentRepository paymentRepository,
    ILogger<PreferenceService> logger
) : IPreferenceService
{
    public async Task<PreferenceResponseDto> CreatePreferenceAsync(
        CreatePreferenceRequestDto request,
        Guid userId
    )
    {
        // 1. Inicia a Transação do Banco de Dados para Atomicidade
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            logger.LogInformation(
                "Iniciando criação de Preferência (Checkout Pro) para Usuário: {UserId}",
                userId
            );

            // 2. Cria a entidade local para gerar o ExternalReference
            var paymentTransaction = new PaymentTransaction(
                userId: userId,
                amount: request.UnitPrice,
                paymentMethodType: "preference",
                planId: request.PlanId
            );

            await paymentRepository.AddAsync(paymentTransaction);

            // 3. Configura a Preferência no SDK do Mercado Pago
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
                // Vínculo com nossa transação local para rastreio no Webhook/BackUrls
                ExternalReference = paymentTransaction.Id.ToString(),

                // Configuração de retorno automático após o pagamento
                AutoReturn = "approved",
                BackUrls = new PreferenceBackUrlsRequest
                {
                    // Substitua pelas URLs reais do seu Dashboard/Frontend
                    Success = "https://bueirointeligente.com/payment/success",
                    Failure = "https://bueirointeligente.com/payment/failure",
                    Pending = "https://bueirointeligente.com/payment/pending",
                },
                // Ativa o modo binário (Aprovado ou Reprovado, sem "em análise") se preferir
                BinaryMode = true,
            };

            // 4. Chama a API via SDK
            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(preferenceRequest);

            if (string.IsNullOrEmpty(preference.Id))
            {
                throw new Exception("O Mercado Pago falhou ao gerar o ID da Preferência.");
            }

            // 5. Atualiza a Entidade Local com os dados da Preferência
            // Usamos o InitPoint (link de checkout) como o nosso TicketUrl
            paymentTransaction.SetPreferenceData(preference.Id, preference.InitPoint);

            await paymentRepository.UpdateAsync(paymentTransaction);

            // 6. Commit da Transação no Banco
            await transaction.CommitAsync();

            logger.LogInformation("Preferência gerada com sucesso. ID: {PrefId}", preference.Id);

            return new PreferenceResponseDto(
                PreferenceId: preference.Id,
                InitPoint: preference.InitPoint,
                ExternalReference: paymentTransaction.Id
            );
        }
        catch (Exception ex)
        {
            // 🛑 ROLLBACK: Se a API do MP falhar, não deixamos uma transação pendente órfã no banco
            logger.LogError(ex, "Erro ao criar preferência no Mercado Pago. Efetuando Rollback.");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
