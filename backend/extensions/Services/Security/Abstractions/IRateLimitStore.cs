namespace backend.extensions.Services.Security.Abstractions;

public interface IRateLimitStore
{
    Task<int?> GetCountAsync(string key, CancellationToken cancellationToken = default);

    Task IncrementAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);
}