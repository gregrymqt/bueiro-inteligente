using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain.Entities;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace backend.Features.Payment.Application.Services;

public class PixService(
    IUnitOfWork unitOfWork,
    IPaymentRepository paymentRepository,
    IMercadoPagoOrderService orderService,
    ILogger<PixService> logger
) : IPixService
{
    public async Task<PixPaymentResponseDto> CreatePixOrderAsync(
        CreatePixRequestDto request,
        Guid userId
    )
    {
        logger.LogInformation("Gerando ordem de Pix para o usuário {UserId}.", userId);

        var response = await unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();

            var paymentTransaction = new PaymentTransaction(
                userId: userId,
                amount: request.Amount,
                paymentMethodType: "pix",
                planId: request.PlanId
            );
            await paymentRepository.AddAsync(paymentTransaction);

            var amount = request.Amount.ToString(
                "F2",
                System.Globalization.CultureInfo.InvariantCulture
            );

            var orderRequest = new MpOrderRequest(
                Type: "online",
                ExternalReference: paymentTransaction.Id.ToString(),
                TotalAmount: amount,
                ProcessingMode: "automatic",
                Payer: new MpOrderPayer(request.PayerEmail),
                Transactions: new MpOrderTransactions(
                    new List<MpOrderPaymentRequest>
                    {
                        new MpOrderPaymentRequest(
                            Amount: amount,
                            PaymentMethod: new MpOrderPaymentMethod("pix", "bank_transfer")
                        )
                    }
                )
            );

            var mpOrder = await orderService.CreateOrderAsync(orderRequest);

            var mpPayment =
                mpOrder.Transactions.Payments.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "A ordem foi criada, mas nenhum pagamento foi gerado."
                );

            var expirationDate = mpPayment.DateOfExpiration ?? DateTimeOffset.UtcNow.AddHours(24);

            paymentTransaction.SetPixData(
                orderId: mpOrder.Id,
                qrCode: mpPayment.PaymentMethod.QrCode,
                qrCodeBase64: mpPayment.PaymentMethod.QrCodeBase64,
                ticketUrl: mpPayment.PaymentMethod.TicketUrl,
                expirationDate: expirationDate
            );

            var paymentId = mpPayment.Id;
            paymentTransaction.UpdateStatus(mpOrder.Status, mpOrder.StatusDetail, paymentId);

            return new PixPaymentResponseDto(
                OrderId: mpOrder.Id,
                PaymentId: paymentId,
                Status: mpOrder.Status,
                StatusDetail: mpOrder.StatusDetail,
                QrCode: mpPayment.PaymentMethod.QrCode,
                QrCodeBase64: mpPayment.PaymentMethod.QrCodeBase64,
                TicketUrl: mpPayment.PaymentMethod.TicketUrl,
                ExpirationDate: expirationDate,
                ExternalReference: paymentTransaction.Id
            );
        });

        logger.LogInformation(
            "Ordem de Pix processada com sucesso. OrderId: {OrderId}, PaymentId: {PaymentId}",
            response.OrderId,
            response.PaymentId
        );

        return response;
    }

    public async Task<bool> RetryPixTransactionAsync(RetryPixRequestDto request, Guid userId)
    {
        logger.LogInformation(
            "Iniciando retry de Pix para Ordem {OrderId}, Transação {TransactionId}",
            request.OrderId,
            request.TransactionId
        );

        // 1. Monta o request de retry (Pix não tem token/parcelas)
        var updateRequest = new MpUpdateTransactionRequest(
            new MpUpdatePaymentMethod(Id: "pix", Type: "bank_transfer")
        );

        // 2. Chama a API
        var success = await orderService.UpdateTransactionAsync(
            request.OrderId,
            request.TransactionId,
            updateRequest
        );

        if (success)
        {
            logger.LogInformation(
                "Retry de Pix enviado com sucesso para a API. Aguardando Webhook."
            );
        }
        else
        {
            logger.LogWarning(
                "Falha ao enviar retry de Pix para a Ordem {OrderId}.",
                request.OrderId
            );
        }

        return success;
    }
}
