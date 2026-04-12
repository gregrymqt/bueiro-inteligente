using Microsoft.AspNetCore.Http;

namespace backend.Extensions.Security.Abstractions;

public interface IRateLimiter
{
    Task EnforceAsync(HttpContext context, CancellationToken cancellationToken = default);
}