namespace backend.extensions.Services.Security.Abstractions;

public interface IRateLimiter
{
    Task EnforceAsync(HttpContext context, CancellationToken cancellationToken = default);
}