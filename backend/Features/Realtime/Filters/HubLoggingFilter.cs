using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Filters;

public sealed class HubLoggingFilter(ILogger<HubLoggingFilter> logger) : IHubFilter
{
    private readonly ILogger<HubLoggingFilter> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        _logger.LogInformation(
            "Hub invoked: {Method}. ConnId: {Id}",
            context.HubMethodName,
            context.Context.ConnectionId
        );

        var result = await next(context).ConfigureAwait(false);

        _logger.LogInformation(
            "Hub completed: {Method}. ConnId: {Id}",
            context.HubMethodName,
            context.Context.ConnectionId
        );

        return result;
    }
}
