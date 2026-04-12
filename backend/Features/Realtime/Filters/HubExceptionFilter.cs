using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Filters;

/// <summary>
/// Logs unhandled hub exceptions and returns a safe HubException to the client.
/// </summary>
public sealed class HubExceptionFilter(ILogger<HubExceptionFilter> logger) : IHubFilter
{
    private const string GenericHubErrorMessage =
        "Ocorreu um erro interno ao processar a solicitação em tempo real.";

    private readonly ILogger<HubExceptionFilter> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        ArgumentNullException.ThrowIfNull(invocationContext);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next(invocationContext).ConfigureAwait(false);
        }
        catch (HubException exception)
        {
            _logger.LogWarning(
                exception,
                "Hub exception in method {HubMethodName}. ConnectionId={ConnectionId}",
                invocationContext.HubMethodName,
                invocationContext.Context.ConnectionId
            );

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled hub exception in method {HubMethodName}. ConnectionId={ConnectionId}",
                invocationContext.HubMethodName,
                invocationContext.Context.ConnectionId
            );

            throw new HubException(GenericHubErrorMessage);
        }
    }
}