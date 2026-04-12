namespace backend.Infrastructure.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T? value, TimeSpan? expiry = null);

    Task RemoveAsync(string key);

    Task<CacheResponseDto<T>> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> fetchFunc,
        TimeSpan? expiry = null
    );
}