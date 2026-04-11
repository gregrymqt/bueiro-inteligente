namespace BueiroInteligente.Infrastructure.Cache;

public record CacheResponseDto<T>(T Data, bool FromCache);
