using backend.extensions.Services.Security.Exceptions;
using backend.extensions.Services.Security.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Presentation.Hubs;

[Authorize]
public sealed class ApplicationHub(WebSocketRateLimiter rateLimiter, ILogger<ApplicationHub> logger)
    : Hub
{
    // ====================================================================
    // 1. GERENCIAMENTO DE CONEXÕES (Fica estritamente no ApplicationHub)
    // ====================================================================

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            try
            {
                await rateLimiter.EnforceAsync(httpContext, Context.ConnectionAborted).ConfigureAwait(false);
            }
            catch (RateLimitExceededException ex)
            {
                throw new HubException(ex.Message);
            }
        }

        logger.LogInformation("Cliente conectado ao Hub Global: {Id}", Context.ConnectionId);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (ex is null)
            logger.LogInformation("WebSocket fechado de forma limpa: {Id}", Context.ConnectionId);
        else
            logger.LogWarning(ex, "WebSocket fechado com erro: {Id}", Context.ConnectionId);

        await base.OnDisconnectedAsync(ex).ConfigureAwait(false);
    }

    // ====================================================================
    // 2. MÉTODOS DE MONITORAMENTO (Com Blindagem e Try/Catch)
    // ====================================================================

    public async Task JoinDrain(string bueiroId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bueiroId))
            {
                logger.LogWarning("Tentativa de entrar no grupo com ID de bueiro vazio.");
                return;
            }

            // Se o Redis estiver offline, o erro vai estourar exatamente nesta linha abaixo
            await Groups.AddToGroupAsync(Context.ConnectionId, bueiroId).ConfigureAwait(false);

            logger.LogInformation("✅ Conexão {ConnectionId} registrada no grupo do bueiro: {BueiroId}", Context.ConnectionId, bueiroId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "🚨 ERRO FATAL ao tentar registrar no grupo {BueiroId}. Verifique se o Redis está online!", bueiroId);
            // Lançar HubException evita que o SignalR feche/corte a conexão WebSocket bruscamente
            throw new HubException("Erro interno no servidor ao tentar conectar na sala do bueiro.");
        }
    }

    public async Task LeaveDrain(string bueiroId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bueiroId))
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, bueiroId).ConfigureAwait(false);
            logger.LogInformation("❌ Conexão {ConnectionId} removida do grupo do bueiro: {BueiroId}", Context.ConnectionId, bueiroId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "🚨 ERRO ao tentar sair do grupo {BueiroId}", bueiroId);
        }
    }
}