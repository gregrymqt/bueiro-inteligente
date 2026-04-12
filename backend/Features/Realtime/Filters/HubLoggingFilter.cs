using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Filters;

/// <summary>
/// Logs hub method invocations and the connection ID for realtime traffic.
/// </summary>
public sealed class HubLoggingFilter(ILogger<HubLoggingFilter> logger) : IHubFilter
{
    private readonly ILogger<HubLoggingFilter> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        ArgumentNullException.ThrowIfNull(invocationContext);
        ArgumentNullException.ThrowIfNull(next);

        string connectionId = invocationContext.Context.ConnectionId;
        string hubMethodName = invocationContext.HubMethodName;

        _logger.LogInformation(
            "Hub method {HubMethodName} invoked. ConnectionId={ConnectionId}",
            hubMethodName,
            connectionId
        );

        object? result = await next(invocationContext).ConfigureAwait(false);

        _logger.LogInformation(
            "Hub method {HubMethodName} completed. ConnectionId={ConnectionId}",
            hubMethodName,
            connectionId
        );

        return result;
    }
}