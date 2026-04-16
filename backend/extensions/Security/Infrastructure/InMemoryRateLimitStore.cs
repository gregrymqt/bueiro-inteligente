using System.Collections.Concurrent;
using backend.Extensions.Security.Abstractions;

namespace backend.Extensions.Security.Infrastructure;

public sealed class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new(
        StringComparer.Ordinal
    );

    public Task<int?> GetCountAsync(string key, CancellationToken ct = default)
    {
        if (!_entries.TryGetValue(key, out var entry))
            return Task.FromResult<int?>(null);

        if (entry.ExpiresAt > DateTimeOffset.UtcNow)
            return Task.FromResult<int?>(entry.Count);

        _entries.TryRemove(key, out _);
        return Task.FromResult<int?>(null);
    }

    public Task IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry(1, now.Add(ttl)),
            (_, existing) =>
                existing.ExpiresAt <= now
                    ? new RateLimitEntry(1, now.Add(ttl))
                    : existing with
                    {
                        Count = existing.Count + 1,
                    }
        );

        return Task.CompletedTask;
    }

    private sealed record RateLimitEntry(int Count, DateTimeOffset ExpiresAt);
}
