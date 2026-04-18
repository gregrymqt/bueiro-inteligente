using System.Net;
using Microsoft.Extensions.Logging;

namespace backend.Features.Rows.Infrastructure.Extensions;

/// <summary>
/// Handler de resiliência para falhas transitórias na API Rows.
/// </summary>
internal sealed class RowsRetryHandler(ILogger<RowsRetryHandler> logger) : DelegatingHandler
{
    // C# 12: Collection Expression para inicialização limpa
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(750),
        TimeSpan.FromSeconds(2),
    ];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct
    )
    {
        if (request.Content is not null)
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);

        Exception? lastException = null;

        for (int attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            try
            {
                var response = await base.SendAsync(request, ct).ConfigureAwait(false);

                if (!IsTransient(response) || attempt == RetryDelays.Length)
                    return response;

                logger.LogWarning(
                    "Tentativa {Attempt} falhou com {Status}. Repetindo...",
                    attempt + 1,
                    (int)response.StatusCode
                );

                response.Dispose();
                await DelayAsync(attempt, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
                when (ex is HttpRequestException or TaskCanceledException
                    && !ct.IsCancellationRequested
                )
            {
                lastException = ex;
                if (attempt == RetryDelays.Length)
                    break;

                logger.LogWarning(ex, "Falha na tentativa {Attempt}. Repetindo...", attempt + 1);
                await DelayAsync(attempt, ct).ConfigureAwait(false);
            }
        }

        throw lastException
            ?? new HttpRequestException("Falha após múltiplas tentativas na API Rows.");
    }

    private static bool IsTransient(HttpResponseMessage res) =>
        res.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
        || (int)res.StatusCode >= 500;

    private async Task DelayAsync(int attempt, CancellationToken ct)
    {
        var delay = RetryDelays[attempt];
        logger.LogInformation("Aguardando {Delay} para nova tentativa.", delay);
        await Task.Delay(delay, ct).ConfigureAwait(false);
    }
}
