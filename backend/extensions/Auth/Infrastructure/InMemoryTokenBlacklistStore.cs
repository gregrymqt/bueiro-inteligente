using System.Collections.Concurrent;
using backend.Extensions.Auth.Abstractions;

namespace backend.Extensions.Auth.Infrastructure;

public sealed class InMemoryTokenBlacklistStore : ITokenBlacklistStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _entries = new(
        StringComparer.Ordinal
    );

    public Task AddAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(ttl);
        _entries[jti] = expiresAt;
        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(jti, out DateTimeOffset expiresAt))
        {
            return Task.FromResult(false);
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(jti, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
