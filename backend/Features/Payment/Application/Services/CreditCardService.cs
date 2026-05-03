using System.Text.Json;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Plan.Application.Base;
using backend.Infrastructure.Persistence; // Ajuste para o namespace do seu AppDbContext
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class CreditCardService(
    IHttpClientFactory httpClientFactory,
    AppDbContext dbContext,
    IPaymentRepository paymentRepository,
    ILogger<CreditCardService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), ICreditCardService
{
    public async Task<CreditCardPaymentResponseDto> CreateCreditCardOrderAsync(
        CreateCreditCardRequestDto request,
        Guid userId
    )
    {
        // 1. Inicia a Transação do Banco de Dados para Atomicidade
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            Logger.LogInformation(
                "Iniciando processamento de Cartão de Crédito para o Usuário: {UserId}",
                userId
            );

            // 2. Cria a entidade no estado Pendente para gerar o ExternalReference
            var paymentTransaction = new PaymentTransaction(
                userId: userId,
                amount: request.Amount,
                paymentMethodType: "credit_card",
                planId: request.PlanId
            );

            await paymentRepository.AddAsync(paymentTransaction);

            // 3. Monta o Payload para a API de Orders (v1/orders)
            // Nota: O token e as parcelas são enviados dentro do objeto payment_method
            var orderRequest = new
            {
                type = "online",
                external_reference = paymentTransaction.Id.ToString(),
                total_amount = request.Amount.ToString(
                    "F2",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                processing_mode = "automatic",
                payer = new { email = request.PayerEmail },
                transactions = new
                {
                    payments = new[]
                    {
                        new
                        {
                            amount = request.Amount.ToString(
                                "F2",
                                System.Globalization.CultureInfo.InvariantCulture
                            ),
                            payment_method = new
                            {
                                id = request.PaymentMethodId,
                                type = "credit_card",
                                token = request.Token,
                                installments = request.Installments,
                            },
                        },
                    },
                },
            };

            // 4. Chamada à API do Mercado Pago via Base Service
            // O SendMercadoPagoRequestAsync já utiliza Polly e injeta a X-Idempotency-Key automaticamente
            var jsonResponse = await SendMercadoPagoRequestAsync(
                HttpMethod.Post,
                "v1/orders",
                orderRequest
            );

            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            // 5. Extração e Validação dos Dados de Resposta
            var orderId = root.GetProperty("id").GetString() ?? string.Empty;
            var status = root.GetProperty("status").GetString() ?? "rejected";
            var statusDetail = root.GetProperty("status_detail").GetString();

            var paymentElement = root.GetProperty("transactions").GetProperty("payments")[0];
            var paymentIdStr = paymentElement.GetProperty("id").GetString() ?? "0";
            long.TryParse(paymentIdStr.Replace("PAY", ""), out long paymentId);

            // Captura URL de validação extra (ex: 3DS), se existir
            string? securityUrl = null;
            if (
                paymentElement
                    .GetProperty("payment_method")
                    .TryGetProperty("transaction_security", out var security)
            )
            {
                securityUrl = security.GetProperty("url").GetString();
            }

            // 6. Atualização da Entidade com os dados do cartão
            // O MP não retorna os 4 últimos dígitos na resposta simplificada de Orders,
            // mas podemos obter via GET posterior ou via SDK se for crucial agora.
            paymentTransaction.SetCreditCardData(paymentId, "****", request.Installments);
            paymentTransaction.UpdateStatus(status, statusDetail);

            await paymentRepository.UpdateAsync(paymentTransaction);

            // 7. Commit da Transação se o status for aceitável
            if (status == "rejected" || status == "cancelled")
            {
                // Em caso de rejeição clara, fazemos rollback para não confirmar a intenção de venda como "sucesso"
                // ou mantemos no banco com status 'rejected' para histórico do usuário.
                // Aqui optamos por confirmar o registro do erro (Commit) e deixar o Service retornar o status.
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.CommitAsync();
            }

            Logger.LogInformation(
                "Pagamento via Cartão processado. Status: {Status}, ID: {Id}",
                status,
                paymentId
            );

            return new CreditCardPaymentResponseDto(
                OrderId: orderId,
                PaymentId: paymentId,
                Status: status,
                StatusDetail: statusDetail ?? string.Empty,
                ExternalResourceUrl: securityUrl,
                ExternalReference: paymentTransaction.Id
            );
        }
        catch (Exception ex)
        {
            // 🛑 ROLLBACK: Se a API falhar ou houver erro de rede, o banco volta ao estado anterior
            Logger.LogError(
                ex,
                "Erro crítico no processamento de Cartão de Crédito. Efetuando Rollback."
            );
            await transaction.RollbackAsync();
            throw;
        }
    }
}
