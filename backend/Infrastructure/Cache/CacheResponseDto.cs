namespace backend.Infrastructure.Cache;

public record CacheResponseDto<T>(T Data, bool FromCache);
