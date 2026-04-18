using System.Text.Json;
using backend.Core;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace backend.Infrastructure.Cache;

public sealed class RedisCacheService(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger
) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // C# 12: Campos capturados diretamente do construtor primário
    private readonly IConnectionMultiplexer _redis =
        connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly ILogger<RedisCacheService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<T?> GetAsync<T>(string key)
    {
        var (found, value) = await TryGetAsync<T>(key).ConfigureAwait(false);
        return found ? value : default;
    }

    public async Task SetAsync<T>(string key, T? value, TimeSpan? expiry = null)
    {
        ValidateKey(key);
        try
        {
            var db = _redis.GetDatabase();
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await db.StringSetAsync(key, payload, (Expiration)expiry!).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar chave '{Key}' no Redis.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        ValidateKey(key);
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover chave '{Key}' do Redis.", key);
        }
    }

    public async Task<CacheResponseDto<T>> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> fetchFunc,
        TimeSpan? expiry = null
    )
    {
        ValidateKey(key);
        ArgumentNullException.ThrowIfNull(fetchFunc);

        var (found, cachedValue) = await TryGetAsync<T>(key).ConfigureAwait(false);
        if (found)
            return new CacheResponseDto<T>(cachedValue!, true);

        T freshData = await fetchFunc().ConfigureAwait(false);
        await SetAsync(key, freshData, expiry).ConfigureAwait(false);

        return new CacheResponseDto<T>(freshData, false);
    }

    private async Task<(bool Found, T? Value)> TryGetAsync<T>(string key)
    {
        ValidateKey(key);
        try
        {
            var cachedValue = await _redis.GetDatabase().StringGetAsync(key).ConfigureAwait(false);
            if (cachedValue.IsNull)
                return (false, default);

            return (true, JsonSerializer.Deserialize<T>(cachedValue!, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar chave '{Key}' do Redis.", key);
            return (false, default);
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw LogicException.InvalidValue(nameof(key), key);
    }
}
