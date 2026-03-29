import json
import logging
from typing import Callable, Awaitable, Optional, Type, TypeVar
from pydantic import BaseModel
import redis.asyncio as redis
from .interfaces import ICacheService
from .dtos import CacheResponseDTO

logger = logging.getLogger(__name__)

T = TypeVar('T', bound=BaseModel)

class RedisCacheService(ICacheService):
    def __init__(self, redis_client: redis.Redis) -> None:
        self._redis = redis_client

    async def get(self, key: str) -> Optional[str]:
        try:
            logger.debug(f"Tentando buscar do cache a chave: {key}")
            value = await self._redis.get(key)
            if value:
                logger.debug(f"Chave encontrada no cache: {key}")
            else:
                logger.debug(f"Chave não encontrada no cache (miss): {key}")
            return value
        except Exception as e:
            logger.error(f"Erro ao buscar chave '{key}' do cache: {str(e)}", exc_info=True)
            return None # Graceful degradation: se falhar o cache, retornamos None

    async def set(self, key: str, value: str, ttl_seconds: Optional[int] = None) -> None:
        try:
            logger.debug(f"Salvando no cache a chave: {key} (TTL: {ttl_seconds}s)")
            if ttl_seconds:
                # setex = Set with Expiration
                await self._redis.setex(key, ttl_seconds, value)
            else:
                await self._redis.set(key, value)
            logger.debug(f"Chave '{key}' salva com sucesso no cache")
        except Exception as e:
            logger.error(f"Erro ao salvar chave '{key}' no cache: {str(e)}", exc_info=True)

    async def delete(self, key: str) -> None:
        try:
            logger.debug(f"Deletando chave do cache: {key}")
            await self._redis.delete(key)
            logger.debug(f"Chave '{key}' deletada com sucesso do cache")
        except Exception as e:
            logger.error(f"Erro ao deletar chave '{key}' do cache: {str(e)}", exc_info=True)

    async def get_or_set(
        self, 
        key: str, 
        fetch_func: Callable[[], Awaitable[T]], 
        model_type: Type[T],
        ttl_seconds: Optional[int] = None
    ) -> CacheResponseDTO[T]:
        try:
            logger.info(f"Iniciando get_or_set para a chave: {key}")
            # 1. Tenta buscar do cache
            cached_data = await self.get(key)
            
            if cached_data:
                try:
                    logger.info(f"Cache hit para a chave: {key}")
                    # Se achou, converte o JSON do Redis de volta para o seu DTO Pydantic.
                    # O IntelliSense vai saber que o retorno é do tipo "T" (seu DTO).
                    parsed_data = model_type.model_validate_json(cached_data)
                    return CacheResponseDTO(data=parsed_data, from_cache=True)
                except Exception as parse_err:
                    logger.error(f"Erro ao parsear dados do cache para a chave {key}: {str(parse_err)}", exc_info=True)
                    # Caso ocorra erro no parse, segue o fluxo para buscar o dado novo

            logger.info(f"Cache miss para a chave: {key}, executando função de fetch")
            # 2. Se não achou no cache (Cache Miss), executa a função que busca do banco/Adafruit
            fresh_data = await fetch_func()
            
            # 3. Salva no cache para a próxima vez, convertendo o DTO para JSON
            logger.info(f"Armazenando novo valor no cache para a chave: {key}")
            await self.set(key, fresh_data.model_dump_json(), ttl_seconds)
            
            return CacheResponseDTO(data=fresh_data, from_cache=False)
        except Exception as e:
            logger.error(f"Erro inesperado no get_or_set para a chave '{key}': {str(e)}", exc_info=True)
            logger.warning(f"Executando fetch_func como fallback para chave '{key}' devido a erro geral no cache")
            fresh_data = await fetch_func()
            return CacheResponseDTO(data=fresh_data, from_cache=False)