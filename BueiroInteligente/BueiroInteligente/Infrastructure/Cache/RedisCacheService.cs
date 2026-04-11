using System.Text.Json;
using BueiroInteligente.Core;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BueiroInteligente.Infrastructure.Cache;

public sealed class RedisCacheService(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger
) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connectionMultiplexer =
        connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

    private readonly ILogger<RedisCacheService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<T?> GetAsync<T>(string key)
    {
        (bool Found, T? Value) result = await TryGetAsync<T>(key).ConfigureAwait(false);
        return result.Found ? result.Value : default;
    }

    public async Task SetAsync<T>(string key, T? value, TimeSpan? expiry = null)
    {
        ValidateKey(key);

        try
        {
            IDatabase database = _connectionMultiplexer.GetDatabase();
            string payload = JsonSerializer.Serialize(value, JsonOptions);
            await database
                .StringSetAsync(key, payload, expiry, false)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao salvar a chave '{Key}' no cache Redis.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        ValidateKey(key);

        try
        {
            IDatabase database = _connectionMultiplexer.GetDatabase();
            await database.KeyDeleteAsync(key).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao remover a chave '{Key}' do cache Redis.", key);
        }
    }

    public async Task<CacheResponseDto<T>> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> fetchFunc,
        TimeSpan? expiry = null
    )
    {
        ValidateKey(key);

        if (fetchFunc is null)
        {
            throw LogicException.NullValue(nameof(fetchFunc));
        }

        (bool Found, T? Value) cachedResult = await TryGetAsync<T>(key).ConfigureAwait(false);

        if (cachedResult.Found)
        {
            return new CacheResponseDto<T>(cachedResult.Value!, true);
        }

        T freshData = await fetchFunc().ConfigureAwait(false);

        try
        {
            await SetAsync(key, freshData, expiry).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Erro ao gravar o resultado do fetch no cache para a chave '{Key}'.",
                key
            );
        }

        return new CacheResponseDto<T>(freshData, false);
    }

    private async Task<(bool Found, T? Value)> TryGetAsync<T>(string key)
    {
        ValidateKey(key);

        try
        {
            IDatabase database = _connectionMultiplexer.GetDatabase();
            RedisValue cachedValue = await database.StringGetAsync(key).ConfigureAwait(false);

            if (cachedValue.IsNull)
            {
                return (false, default);
            }

            T? value = JsonSerializer.Deserialize<T>(cachedValue!, JsonOptions);
            return (true, value);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Erro ao desserializar a chave '{Key}' do cache Redis.",
                key
            );
            return (false, default);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao buscar a chave '{Key}' do cache Redis.", key);
            return (false, default);
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw LogicException.InvalidValue(nameof(key), key);
        }
    }
}