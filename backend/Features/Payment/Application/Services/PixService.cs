using System.Text.Json;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Entities;
using backend.Features.Payment.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace backend.Features.Payment.Application.Services;

public class PixService(
    AppDbContext dbContext,
    IPaymentRepository paymentRepository,
    IMercadoPagoOrderService orderService,
    ILogger<PixService> logger
) : IPixService
{
    public async Task<PixPaymentResponseDto> CreatePixOrderAsync(CreatePixRequestDto request, Guid userId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            logger.LogInformation("Gerando Ordem de Pix para o Usuário: {UserId}", userId);

            var paymentTransaction = new PaymentTransaction(
                userId: userId, amount: request.Amount, paymentMethodType: "pix", planId: request.PlanId
            );
            await paymentRepository.AddAsync(paymentTransaction);

            var orderRequest = new MpOrderRequest(
                Type: "online",
                ExternalReference: paymentTransaction.Id.ToString(),
                TotalAmount: request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                ProcessingMode: "automatic",
                Payer: new MpOrderPayer(request.PayerEmail),
                Transactions: new MpOrderTransactions(
                    new List<MpOrderPaymentRequest>
                    {
                        new MpOrderPaymentRequest(
                            Amount: request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            PaymentMethod: new MpOrderPaymentMethod("pix", "bank_transfer")
                        )
                    }
                )
            );

            // Chamada Limpa para o OrderService!
            var mpOrder = await orderService.CreateOrderAsync(orderRequest);

            var mpPayment = mpOrder.Transactions.Payments.FirstOrDefault()
                            ?? throw new Exception("A ordem foi criada, mas nenhum pagamento foi gerado.");

            var expirationDate = mpPayment.DateOfExpiration ?? DateTimeOffset.UtcNow.AddHours(24);

            paymentTransaction.SetPixData(
                orderId: mpOrder.Id,
                qrCode: mpPayment.PaymentMethod.QrCode,
                qrCodeBase64: mpPayment.PaymentMethod.QrCodeBase64,
                ticketUrl: mpPayment.PaymentMethod.TicketUrl,
                expirationDate: expirationDate
            );

            long.TryParse(mpPayment.Id.Replace("PAY", ""), out long paymentIdLong);
            paymentTransaction.UpdateStatus(mpOrder.Status, mpOrder.StatusDetail, paymentIdLong);

            await paymentRepository.UpdateAsync(paymentTransaction);
            await transaction.CommitAsync();

            return new PixPaymentResponseDto(
                OrderId: mpOrder.Id,
                PaymentId: paymentIdLong,
                Status: mpOrder.Status,
                StatusDetail: mpOrder.StatusDetail,
                QrCode: mpPayment.PaymentMethod.QrCode,
                QrCodeBase64: mpPayment.PaymentMethod.QrCodeBase64,
                TicketUrl: mpPayment.PaymentMethod.TicketUrl,
                ExpirationDate: expirationDate,
                ExternalReference: paymentTransaction.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao gerar ordem de Pix. Executando Rollback.");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RetryPixTransactionAsync(RetryPixRequestDto request, Guid userId)
    {
        logger.LogInformation("Iniciando retry de Pix para Ordem {OrderId}, Transação {TransactionId}", request.OrderId,
            request.TransactionId);

        // 1. Monta o request de retry (Pix não tem token/parcelas)
        var updateRequest = new MpUpdateTransactionRequest(
            new MpUpdatePaymentMethod(
                Id: "pix",
                Type: "bank_transfer"
            )
        );

        // 2. Chama a API
        var success = await orderService.UpdateTransactionAsync(request.OrderId, request.TransactionId, updateRequest);

        if (success)
        {
            logger.LogInformation("Retry de Pix enviado com sucesso para a API. Aguardando Webhook.");
        }
        else
        {
            logger.LogWarning("Falha ao enviar retry de Pix para a Ordem {OrderId}.", request.OrderId);
        }

        return success;
    }
}