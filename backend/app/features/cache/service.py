import json
from typing import Callable, Awaitable, Optional, Type, TypeVar
from pydantic import BaseModel
import redis.asyncio as redis
from .interfaces import ICacheService

T = TypeVar('T', bound=BaseModel)

class RedisCacheService:
    def __init__(self, redis_client: redis.Redis) -> None:
        self._redis = redis_client

    async def get(self, key: str) -> Optional[str]:
        return await self._redis.get(key)

    async def set(self, key: str, value: str, ttl_seconds: Optional[int] = None) -> None:
        if ttl_seconds:
            # setex = Set with Expiration
            await self._redis.setex(key, ttl_seconds, value)
        else:
            await self._redis.set(key, value)

    async def delete(self, key: str) -> None:
        await self._redis.delete(key)

    async def get_or_set(
        self, 
        key: str, 
        fetch_func: Callable[[], Awaitable[T]], 
        model_type: Type[T],
        ttl_seconds: Optional[int] = None
    ) -> T:
        # 1. Tenta buscar do cache
        cached_data = await self.get(key)
        
        if cached_data:
            # Se achou, converte o JSON do Redis de volta para o seu DTO Pydantic.
            # O IntelliSense vai saber que o retorno é do tipo "T" (seu DTO).
            return model_type.model_validate_json(cached_data)

        # 2. Se não achou no cache (Cache Miss), executa a função que busca do banco/Adafruit
        fresh_data = await fetch_func()
        
        # 3. Salva no cache para a próxima vez, convertendo o DTO para JSON
        await self.set(key, fresh_data.model_dump_json(), ttl_seconds)
        
        return fresh_data