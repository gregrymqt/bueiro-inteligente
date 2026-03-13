import redis.asyncio as redis  # <-- A mágica do assíncrono está aqui
from .config import settings

# Montando a URL corretamente
redis_url = f"redis://{settings.REDIS_HOST}:{settings.REDIS_PORT}/{settings.REDIS_DB}"
if settings.REDIS_PASSWORD:
    redis_url = f"redis://:{settings.REDIS_PASSWORD}@{settings.REDIS_HOST}:{settings.REDIS_PORT}/{settings.REDIS_DB}"

# Cria o pool de conexão assíncrona
redis_client = redis.from_url(
    redis_url, 
    decode_responses=True
)

def get_cache() -> redis.Redis:
    """
    Retorna o cliente do Redis tipado para o IntelliSense
    """
    return redis_client