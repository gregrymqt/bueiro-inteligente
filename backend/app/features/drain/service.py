import logging
from typing import List
from fastapi import HTTPException

from .interfaces import IDrainRepository, IDrainService
from .dto import DrainCreate, DrainUpdate, DrainRead

logger = logging.getLogger(__name__)

class DrainService(IDrainService):
    def __init__(self, repository: IDrainRepository):
        self._repository = repository

    async def get_all_drains(self, skip: int = 0, limit: int = 100) -> List[DrainRead]:
        drains = await self._repository.get_all(skip=skip, limit=limit)
        return [DrainRead.model_validate(drain) for drain in drains]

    async def get_drain_by_id(self, drain_id: int) -> DrainRead:
        drain = await self._repository.get_by_id(drain_id)
        if not drain:
            raise HTTPException(status_code=404, detail="Drain não encontrado")
        return DrainRead.model_validate(drain)

    async def create_drain(self, data: DrainCreate) -> DrainRead:
        # Validar hardware_id unico
        existing_drain = await self._repository.get_by_hardware_id(data.hardware_id)
        if existing_drain:
            raise HTTPException(
                status_code=400,
                detail=f"hardware_id '{data.hardware_id}' já está em uso."
            )

        new_drain = await self._repository.create(data)
        return DrainRead.model_validate(new_drain)

    async def update_drain(self, drain_id: int, data: DrainUpdate) -> DrainRead:
        drain = await self._repository.get_by_id(drain_id)
        if not drain:
            raise HTTPException(status_code=404, detail="Drain não encontrado")

        # Se estiver atualizando o hardware_id, precisa checar unicidade
        if data.hardware_id and data.hardware_id != drain.hardware_id:
            existing_drain = await self._repository.get_by_hardware_id(data.hardware_id)
            if existing_drain:
                 raise HTTPException(
                    status_code=400,
                    detail=f"hardware_id '{data.hardware_id}' já está em uso."
                )

        updated_drain = await self._repository.update(drain, data)
        return DrainRead.model_validate(updated_drain)

    async def delete_drain(self, drain_id: int) -> None:
        drain = await self._repository.get_by_id(drain_id)
        if not drain:
            raise HTTPException(status_code=404, detail="Drain não encontrado")

        await self._repository.delete(drain)
