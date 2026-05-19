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

            // ✅ Fortemente Tipado: Monta o payload completo utilizando os DTOs do ecossistema
            var mpOrderPayload = new MpOrderRequest(
                Type: "online",
                ExternalReference: paymentTransaction.Id.ToString(), // ID do nosso banco para idempotência
                TotalAmount: request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                ProcessingMode: "automatic",
                Payer: new MpOrderPayer(
                    Email: request.PayerEmail,
                    FirstName: !string.IsNullOrWhiteSpace(request.FirstName) ? request.FirstName : "Titular", // Evita string vazia
                    LastName: !string.IsNullOrWhiteSpace(request.LastName) ? request.LastName : "Cartao",    // Evita string vazia
                    Identification: new MpOrderIdentification(
                        Type: request.IdentificationType ?? "CPF",
                        Number: request.IdentificationNumber?.Replace(".", "").Replace("-", "") ?? "" // Limpa o CPF
                    )
                ),
                Transactions: new MpOrderTransactions(
                    Payments: new List<MpOrderPaymentRequest>
                    {
                        new MpOrderPaymentRequest(
                            Amount: request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            PaymentMethod: new MpOrderPaymentMethod(
                                Id: request.PaymentMethodId,
                                Type: "credit_card",
                                Token: request.Token,             // 🔥 Injetando o token do SDK do Front
                                Installments: request.Installments // 🔥 Injetando as parcelas escolhidas
                            ),
                            ExpirationTime: null // 🔥 Forçando NULL para o Mercado Pago aceitar o Cartão
                        )
                    }
                )
            );

            MpOrderResponse? mpOrder = null;
            try
            {
                mpOrder = await orderService.CreateOrderAsync(mpOrderPayload);
            }
            catch (Exception ex)
            {   
                logger.LogWarning(ex, "O gateway de pagamento recusou a transação (ex: high_risk) ou falhou. Marcando como rejeitado localmente.");

                // Atualizamos a entidade local impedindo o rollback do UnitOfWork!
                paymentTransaction.SetCreditCardData("0", "****", request.Installments);
                paymentTransaction.UpdateStatus("rejected", "cc_rejected_high_risk");

                // Retornamos um DTO indicando falha de negócio, sem estourar Erro 500
                return new CreditCardPaymentResponseDto(
                    OrderId: string.Empty,
                    PaymentId: "0",
                    Status: "rejected",
                    StatusDetail: "high_risk",
                    ExternalResourceUrl: null,
                    ExternalReference: paymentTransaction.Id
                );
            }

            // Fluxo de sucesso: Se não estourou exceção, o gateway processou normalmente
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

        var updateRequest = new MpUpdateTransactionRequest(
            new MpUpdatePaymentMethod(
                Id: request.PaymentMethodId,
                Type: "credit_card",
                Token: request.Token,
                Installments: request.Installments
            )
        );

        var success = await orderService.UpdateTransactionAsync(request.OrderId, request.TransactionId, updateRequest);

        if (success)
        {
            logger.LogInformation("Retry enviado com sucesso para a API. Aguardando Webhook.");
        }
        else
        {
            logger.LogWarning("Falha ao enviar retry para a Ordem {OrderId}.", request.OrderId);
        }

        return success;
    }
}