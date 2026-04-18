namespace backend.Infrastructure.Cache;

/// <summary>
/// Wrapper para respostas de cache indicando a origem do dado.
/// </summary>
public sealed record CacheResponseDto<T>(T Data, bool FromCache);
