using System.Text.Json;
using backend.Features.Payment.Application.DTOs;
using backend.Features.Payment.Application.Interfaces;
using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Plan.Application.Base;
using backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace backend.Features.Payment.Application.Services;

public class PixService(
    IHttpClientFactory httpClientFactory,
    AppDbContext dbContext,
    IPaymentRepository paymentRepository,
    ILogger<PixService> logger
) : MercadoPagoServiceBase(httpClientFactory, logger), IPixService
{
    public async Task<PixPaymentResponseDto> CreatePixOrderAsync(
        CreatePixRequestDto request,
        Guid userId
    )
    {
        // 1. Inicia Transação DB para Atomicidade
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            Logger.LogInformation(
                "Gerando Ordem de Pix (v1/orders) para o Usuário: {UserId}",
                userId
            );

            // 2. Persistência Local
            var paymentTransaction = new PaymentTransaction(
                userId: userId,
                amount: request.Amount,
                paymentMethodType: "pix",
                planId: request.PlanId
            );

            await paymentRepository.AddAsync(paymentTransaction);

            // 3. Montagem do Payload para a API Orders
            var orderRequest = new MpOrderRequest(
                Type: "online",
                ExternalReference: paymentTransaction.Id.ToString(),
                TotalAmount: request.Amount.ToString(
                    "F2",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                ProcessingMode: "automatic",
                Payer: new MpOrderPayer(request.PayerEmail),
                Transactions: new MpOrderTransactions(
                    new List<MpOrderPaymentRequest>
                    {
                        new MpOrderPaymentRequest(
                            Amount: request.Amount.ToString(
                                "F2",
                                System.Globalization.CultureInfo.InvariantCulture
                            ),
                            PaymentMethod: new MpOrderPaymentMethod("pix", "bank_transfer")
                        ),
                    }
                )
            );

            // 4. Chamada via Base Service (Utiliza o Polly e Idempotência internos)
            // Nota: O SendMercadoPagoRequestAsync já gera uma X-Idempotency-Key por padrão
            var jsonResponse = await SendMercadoPagoRequestAsync(
                HttpMethod.Post,
                "v1/orders",
                orderRequest
            );

            var mpOrder =
                JsonSerializer.Deserialize<MpOrderResponse>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? throw new Exception("Falha ao processar a resposta do Mercado Pago.");

            // 5. Extração dos dados do primeiro pagamento da ordem
            var mpPayment =
                mpOrder.Transactions.Payments.FirstOrDefault()
                ?? throw new Exception("A ordem foi criada, mas nenhum pagamento foi gerado.");

            var expirationDate = mpPayment.DateOfExpiration ?? DateTimeOffset.UtcNow.AddHours(24);

            // 6. Atualização da Entidade Local
            paymentTransaction.SetPixData(
                orderId: mpOrder.Id,
                qrCode: mpPayment.PaymentMethod.QrCode,
                qrCodeBase64: mpPayment.PaymentMethod.QrCodeBase64,
                ticketUrl: mpPayment.PaymentMethod.TicketUrl,
                expirationDate: expirationDate
            );

            // Tenta converter o ID do pagamento para long (compatibilidade com a entidade)
            long.TryParse(mpPayment.Id.Replace("PAY", ""), out long paymentIdLong);

            paymentTransaction.UpdateStatus(mpOrder.Status, mpOrder.StatusDetail, paymentIdLong);

            await paymentRepository.UpdateAsync(paymentTransaction);

            // 7. Commit da Transação
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
            Logger.LogError(ex, "Erro ao gerar ordem de Pix. Executando Rollback.");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
