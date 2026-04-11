import logging
from typing import List, Optional
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select

from .interfaces import IDrainRepository
from .dto import DrainCreate, DrainUpdate
from .models import Drain

logger = logging.getLogger(__name__)

class DrainRepository(IDrainRepository):
    def __init__(self, db_client: AsyncSession):
        self._db = db_client

    async def get_by_id(self, drain_id: int) -> Optional[Drain]:
        try:
            stmt = select(Drain).where(Drain.id == drain_id)
            result = await self._db.execute(stmt)
            return result.scalars().first()
        except Exception as e:
            logger.error(f"Erro ao buscar Drain por ID {drain_id}: {str(e)}")
            raise

    async def get_by_hardware_id(self, hardware_id: str) -> Optional[Drain]:
        try:
            stmt = select(Drain).where(Drain.hardware_id == hardware_id)
            result = await self._db.execute(stmt)
            return result.scalars().first()
        except Exception as e:
            logger.error(f"Erro ao buscar Drain por hardware_id {hardware_id}: {str(e)}")
            raise

    async def get_all(self, skip: int = 0, limit: int = 100) -> List[Drain]:
        try:
            stmt = select(Drain).offset(skip).limit(limit)
            result = await self._db.execute(stmt)
            return list(result.scalars().all())
        except Exception as e:
            logger.error(f"Erro ao buscar todos Drains: {str(e)}")
            raise

    async def create(self, data: DrainCreate) -> Drain:
        try:
            db_drain = Drain(
                name=data.name,
                address=data.address,
                latitude=data.latitude,
                longitude=data.longitude,
                is_active=data.is_active,
                hardware_id=data.hardware_id
            )
            self._db.add(db_drain)
            await self._db.commit()
            await self._db.refresh(db_drain)
            return db_drain
        except Exception as e:
            await self._db.rollback()
            logger.error(f"Erro ao criar Drain: {str(e)}")
            raise

    async def update(self, drain: Drain, data: DrainUpdate) -> Drain:
        try:
            update_data = data.model_dump(exclude_unset=True)
            for key, value in update_data.items():
                setattr(drain, key, value)

            await self._db.commit()
            await self._db.refresh(drain)
            return drain
        except Exception as e:
            await self._db.rollback()
            logger.error(f"Erro ao atualizar Drain {drain.id}: {str(e)}")
            raise

    async def delete(self, drain: Drain) -> None:
        try:
            await self._db.delete(drain)
            await self._db.commit()
        except Exception as e:
            await self._db.rollback()
            logger.error(f"Erro ao deletar Drain {drain.id}: {str(e)}")
            raise
