namespace backend.Extensions.Auth.Abstractions;

public interface ITokenBlacklistStore
{
    Task AddAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default);
}
