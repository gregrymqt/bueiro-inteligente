using System.Net;
using Microsoft.Extensions.Logging;

namespace backend.Features.Rows.Infrastructure.Extensions;

/// <summary>
/// Retry handler for transient Rows API failures.
/// </summary>
internal sealed class RowsRetryHandler(ILogger<RowsRetryHandler> logger) : DelegatingHandler
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(750),
        TimeSpan.FromSeconds(2),
    ];

    private readonly ILogger<RowsRetryHandler> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        }

        Exception? lastException = null;

        for (int attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken)
                    .ConfigureAwait(false);

                if (!IsTransient(response) || attempt == RetryDelays.Length)
                {
                    return response;
                }

                _logger.LogWarning(
                    "Rows API respondeu com {StatusCode} na tentativa {Attempt}. Repetindo a requisição.",
                    (int)response.StatusCode,
                    attempt + 1
                );

                response.Dispose();
                await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                lastException = exception;

                if (attempt == RetryDelays.Length)
                {
                    break;
                }

                _logger.LogWarning(
                    exception,
                    "Timeout ao chamar Rows na tentativa {Attempt}. Repetindo a requisição.",
                    attempt + 1
                );

                await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                lastException = exception;

                if (attempt == RetryDelays.Length)
                {
                    break;
                }

                _logger.LogWarning(
                    exception,
                    "Falha transitória ao chamar Rows na tentativa {Attempt}. Repetindo a requisição.",
                    attempt + 1
                );

                await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
        }

        throw lastException
            ?? new HttpRequestException("Falha ao chamar a API Rows após as tentativas de retry.");
    }

    private static bool IsTransient(HttpResponseMessage response)
    {
        int statusCode = (int)response.StatusCode;

        return response.StatusCode == HttpStatusCode.RequestTimeout
            || response.StatusCode == HttpStatusCode.TooManyRequests
            || statusCode >= 500;
    }

    private Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken)
    {
        TimeSpan delay = RetryDelays[attempt];
        _logger.LogInformation("Aguardando {Delay} antes da próxima tentativa da API Rows.", delay);
        return Task.Delay(delay, cancellationToken);
    }
}
