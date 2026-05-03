using backend.Features.Payment.Domain.Entities;

namespace backend.Features.Payment.Application.Services;


using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Persistence; // Ajuste para o namespace do seu AppDbContext
using Microsoft.Extensions.Logging;
using static System.Int64;


public class CreditCardService(
    AppDbContext dbContext,
    IPaymentRepository paymentRepository,
    ILogger<CreditCardService> logger,
    IMercadoPagoOrderService orderService
) : ICreditCardService
{
    public async Task<CreditCardPaymentResponseDto> CreateCreditCardOrderAsync(CreateCreditCardRequestDto request,
        Guid userId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            logger.LogInformation("Iniciando processamento de Cartão de Crédito para o Usuário: {UserId}", userId);

            var paymentTransaction = new PaymentTransaction(
                userId: userId, amount: request.Amount, paymentMethodType: "credit_card", planId: request.PlanId
            );
            await paymentRepository.AddAsync(paymentTransaction);

            var orderRequest = new
            {
                type = "online",
                external_reference = paymentTransaction.Id.ToString(),
                total_amount = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                processing_mode = "automatic",
                payer = new { email = request.PayerEmail },
                transactions = new
                {
                    payments = new[]
                    {
                        new
                        {
                            amount = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            payment_method = new
                            {
                                id = request.PaymentMethodId, type = "credit_card", token = request.Token,
                                installments = request.Installments
                            }
                        }
                    }
                }
            };

            // Chamada Limpa para o OrderService!
            var mpOrder = await orderService.CreateOrderAsync(orderRequest);

            var status = string.IsNullOrEmpty(mpOrder.Status) ? "rejected" : mpOrder.Status;
            var statusDetail = mpOrder.StatusDetail;

            var paymentElement = mpOrder.Transactions.Payments.FirstOrDefault();
            TryParse(paymentElement?.Id?.Replace("PAY", ""), out var paymentId);

            paymentTransaction.SetCreditCardData(paymentId, "****", request.Installments);
            paymentTransaction.UpdateStatus(status, statusDetail);

            await paymentRepository.UpdateAsync(paymentTransaction);

            if (status is "rejected" or "cancelled")
            {
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            else
            {
                await transaction.CommitAsync();
            }

            logger.LogInformation("Pagamento via Cartão processado. Status: {Status}, ID: {Id}", status, paymentId);

            return new CreditCardPaymentResponseDto(
                OrderId: mpOrder.Id,
                PaymentId: paymentId,
                Status: status,
                StatusDetail: statusDetail ?? string.Empty,
                ExternalResourceUrl: null, // Para 3DS via DTO, adicionaremos no futuro se necessário.
                ExternalReference: paymentTransaction.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro crítico no processamento de Cartão de Crédito. Efetuando Rollback.");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RetryCreditCardTransactionAsync(RetryCreditCardRequestDto request, Guid userId)
    {
        logger.LogInformation("Iniciando retry de Cartão de Crédito para Ordem {OrderId}, Transação {TransactionId}",
            request.OrderId, request.TransactionId);

        // 1. Monta o request esperado pelo PUT v1/orders/{id}/transactions/{id}
        var updateRequest = new MpUpdateTransactionRequest(
            new MpUpdatePaymentMethod(
                Id: request.PaymentMethodId,
                Type: "credit_card",
                Token: request.Token,
                Installments: request.Installments
            )
        );

        // 2. Chama a API do Mercado Pago
        var success = await orderService.UpdateTransactionAsync(request.OrderId, request.TransactionId, updateRequest);

        if (success)
        {
            // Opcional: Você pode atualizar o status no banco local para "pending" 
            // enquanto aguarda o webhook do Hangfire com o resultado do processamento.
            logger.LogInformation("Retry enviado com sucesso para a API. Aguardando Webhook.");
        }
        else
        {
            logger.LogWarning("Falha ao enviar retry para a Ordem {OrderId}.", request.OrderId);
        }

        return success;
    }
}