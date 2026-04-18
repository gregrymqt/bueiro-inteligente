using Microsoft.AspNetCore.SignalR;

namespace backend.Features.Realtime.Filters;

public sealed class HubExceptionFilter(ILogger<HubExceptionFilter> logger) : IHubFilter
{
    private const string GenericHubErrorMessage =
        "Ocorreu um erro interno ao processar a solicitação em tempo real.";
    private readonly ILogger<HubExceptionFilter> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next(context).ConfigureAwait(false);
        }
        catch (HubException ex)
        {
            _logger.LogWarning(
                ex,
                "Hub exception: {Method}. ConnId: {Id}",
                context.HubMethodName,
                context.Context.ConnectionId
            );
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled hub exception: {Method}. ConnId: {Id}",
                context.HubMethodName,
                context.Context.ConnectionId
            );
            throw new HubException(GenericHubErrorMessage);
        }
    }
}
