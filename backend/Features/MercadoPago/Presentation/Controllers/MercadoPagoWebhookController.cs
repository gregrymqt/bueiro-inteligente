using backend.Features.MercadoPago.Application.DTOs;
using backend.Features.MercadoPago.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.MercadoPago.Presentation.Controllers;

[Route("webhooks")]
[AllowAnonymous] // Webhooks devem ser acessíveis externamente (sem token JWT)
public class MercadoPagoWebhookController : ApiControllerBase
{
    private readonly ILogger<MercadoPagoWebhookController> _logger;
    private readonly IWebhookService _webhookService;

    public MercadoPagoWebhookController(
        ILogger<MercadoPagoWebhookController> logger,
        IWebhookService webhookService
    )
    {
        _logger = logger;
        _webhookService = webhookService;
    }

    [HttpPost("mercadopago")]
    public async Task<IActionResult> ReceiveWebhook(
        [FromBody] MercadoPagoWebhookNotification notification
    )
    {
        _logger.LogInformation(
            "Webhook recebido: Tipo={Type}, Action={Action}, Data.Id={Id}",
            notification.Type,
            notification.Action,
            notification.Data?.Id
        );

        try
        {
            if (!_webhookService.IsSignatureValid(Request, notification))
            {
                return BadRequest(new { error = "Assinatura do webhook inválida." });
            }

            await _webhookService.ProcessWebhookNotificationAsync(notification);

            // O Mercado Pago exige um código 2xx rapidamente (Accepted 202 ou OK 200)
            return Accepted(new { status = "received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar o Webhook do Mercado Pago.");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
