from typing import Protocol, TypeVar, Callable, Awaitable, Optional, Type
from pydantic import BaseModel
from .dtos import CacheResponseDTO

# Criamos um "Generic" que obriga a ser um modelo do Pydantic (DTO)
T = TypeVar('T', bound=BaseModel)

class ICacheService(Protocol):
    async def get(self, key: str) -> Optional[str]:
        """Busca uma string pura no cache"""
        ...

    async def set(self, key: str, value: str, ttl_seconds: Optional[int] = None) -> None:
        """Salva uma string no cache, com tempo de expiração opcional"""
        ...

    async def delete(self, key: str) -> None:
        """Remove uma chave do cache"""
        ...

    async def get_or_set(
        self, 
        key: str, 
        fetch_func: Callable[[], Awaitable[T]], 
        model_type: Type[T],
        ttl_seconds: Optional[int] = None
    ) -> CacheResponseDTO[T]:
        """
        Tenta buscar do cache. Se não existir, executa a função (fetch_func),
        salva no cache e retorna o DTO fortemente tipado dentro de um CacheResponseDTO.
        """
        ...