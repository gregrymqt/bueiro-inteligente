using backend.extensions.Services.Security.Abstractions;
using backend.extensions.Services.Security.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace backend.Infrastructure.Filters;

public sealed class RateLimitFilter(IRateLimiter rateLimiter) : IAsyncActionFilter
{
    private readonly IRateLimiter _rateLimiter =
        rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await _rateLimiter.EnforceAsync(context.HttpContext).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }
        catch (RateLimitExceededException exception)
        {
            if (exception.RetryAfter is { } retryAfter)
            {
                context.HttpContext.Response.Headers[HeaderNames.RetryAfter] = (
                    (int)retryAfter.TotalSeconds
                ).ToString();
            }

            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Title = "Too many requests",
                    Detail = exception.Message,
                    Status = StatusCodes.Status429TooManyRequests,
                    Type = "https://httpstatuses.com/429",
                }
            )
            {
                StatusCode = StatusCodes.Status429TooManyRequests,
            };
        }
    }
}
