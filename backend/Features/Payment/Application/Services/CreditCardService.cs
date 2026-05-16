using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain.Entities;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class CreditCardService(
    IUnitOfWork unitOfWork,
    IPaymentRepository paymentRepository,
    ILogger<CreditCardService> logger,
    IMercadoPagoOrderService orderService
) : ICreditCardService
{
    public async Task<CreditCardPaymentResponseDto> CreateCreditCardOrderAsync(
        CreateCreditCardRequestDto request,
        Guid userId
    )
    {
        logger.LogInformation(
            "Iniciando processamento de Cartão de Crédito para o usuário {UserId}.",
            userId
        );

        var response = await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();

            var paymentTransaction = new PaymentTransaction(
                userId: userId,
                amount: request.Amount,
                paymentMethodType: "credit_card",
                planId: request.PlanId
            );
            await paymentRepository.AddAsync(paymentTransaction);

            var amount = request.Amount.ToString(
                "F2",
                System.Globalization.CultureInfo.InvariantCulture
            );

            var orderRequest = new
            {
                type = "online",
                external_reference = paymentTransaction.Id.ToString(),
                total_amount = amount,
                processing_mode = "automatic",
                payer = new { email = request.PayerEmail },
                transactions = new
                {
                    payments = new[]
                    {
                        new
                        {
                            amount,
                            payment_method = new
                            {
                                id = request.PaymentMethodId,
                                type = "credit_card",
                                token = request.Token,
                                installments = request.Installments
                            }
                        }
                    }
                }
            };

            var mpOrder = await orderService.CreateOrderAsync(orderRequest);

            var status = string.IsNullOrEmpty(mpOrder.Status) ? "rejected" : mpOrder.Status;
            var statusDetail = mpOrder.StatusDetail;
            var paymentElement = mpOrder.Transactions.Payments.FirstOrDefault();
            var paymentId = paymentElement?.Id ?? "0";

            paymentTransaction.SetCreditCardData(paymentId, "****", request.Installments);
            paymentTransaction.UpdateStatus(status, statusDetail);

            return new CreditCardPaymentResponseDto(
                OrderId: mpOrder.Id,
                PaymentId: paymentId,
                Status: status,
                StatusDetail: statusDetail ?? string.Empty,
                ExternalResourceUrl: null,
                ExternalReference: paymentTransaction.Id
            );
        });

        logger.LogInformation(
            "Pagamento via Cartão processado. Status: {Status}, ID: {PaymentId}",
            response.Status,
            response.PaymentId
        );

        return response;
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