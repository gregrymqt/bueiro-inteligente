from typing import TypeVar, Generic
from pydantic import BaseModel

T = TypeVar('T', bound=BaseModel)

class CacheResponseDTO(BaseModel, Generic[T]):
    data: T
    from_cache: bool
    
