using System.Collections.Concurrent;
using backend.Extensions.Security.Abstractions;

namespace backend.Extensions.Security.Infrastructure;

public sealed class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new(
        StringComparer.Ordinal
    );

    public Task<int?> GetCountAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(key, out RateLimitEntry? entry))
        {
            return Task.FromResult<int?>(null);
        }

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return Task.FromResult<int?>(null);
        }

        return Task.FromResult<int?>(entry.Count);
    }

    public Task IncrementAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry(1, now.Add(ttl)),
            (_, existing) =>
            {
                if (existing.ExpiresAt <= now)
                {
                    return new RateLimitEntry(1, now.Add(ttl));
                }

                return existing with
                {
                    Count = existing.Count + 1,
                };
            }
        );

        return Task.CompletedTask;
    }

    private sealed record RateLimitEntry(int Count, DateTimeOffset ExpiresAt);
}