from typing import List, Optional, Protocol
from .dto import DrainCreate, DrainUpdate, DrainRead
from .models import Drain

class IDrainRepository(Protocol):
    async def get_by_id(self, drain_id: int) -> Optional[Drain]:
        ...

    async def get_by_hardware_id(self, hardware_id: str) -> Optional[Drain]:
        ...

    async def get_all(self, skip: int = 0, limit: int = 100) -> List[Drain]:
        ...

    async def create(self, data: DrainCreate) -> Drain:
        ...

    async def update(self, drain: Drain, data: DrainUpdate) -> Drain:
        ...

    async def delete(self, drain: Drain) -> None:
        ...

class IDrainService(Protocol):
    async def get_all_drains(self, skip: int = 0, limit: int = 100) -> List[DrainRead]:
        ...

    async def get_drain_by_id(self, drain_id: int) -> DrainRead:
        ...

    async def create_drain(self, data: DrainCreate) -> DrainRead:
        ...

    async def update_drain(self, drain_id: int, data: DrainUpdate) -> DrainRead:
        ...

    async def delete_drain(self, drain_id: int) -> None:
        ...
