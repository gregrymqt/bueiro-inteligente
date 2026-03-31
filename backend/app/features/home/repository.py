import uuid
import asyncio
from typing import Dict, Any, Optional, Tuple
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select

from app.features.home.models import CarouselModel, StatCardModel
from app.features.home.dto import (
    CarouselCreateDTO, CarouselUpdateDTO,
    StatCardCreateDTO, StatCardUpdateDTO
)

class HomeRepository:
    def __init__(self, db_session: AsyncSession):
        self.db = db_session

    async def get_all_content(self) -> Dict[str, Any]:
        """Busca todos os carousels e stats ordenados."""
        carousels_stmt = select(CarouselModel).order_by(CarouselModel.order)
        stats_stmt = select(StatCardModel).order_by(StatCardModel.order)

        result_carousels, result_stats = await asyncio.gather(
            self.db.execute(carousels_stmt),
            self.db.execute(stats_stmt)
        )

        return {
            "carousels": result_carousels.scalars().all(),
            "stats": result_stats.scalars().all()
        }

    # ==========================================
    # Carousel Operations
    # ==========================================

    async def create_carousel_item(self, data: CarouselCreateDTO) -> CarouselModel:
        # Extrai serializando URLs para strings (compatibility with SQLAlchemy via Pydantic v2)
        payload = data.model_dump(mode="json")
        new_carousel = CarouselModel(**payload)
        self.db.add(new_carousel)
        await self.db.commit()
        await self.db.refresh(new_carousel)
        return new_carousel

    async def update_carousel_item(self, item_id: uuid.UUID, data: CarouselUpdateDTO) -> Optional[CarouselModel]:
        result = await self.db.execute(select(CarouselModel).where(CarouselModel.id == item_id))
        carousel = result.scalars().first()
        
        if carousel:
            # exclude_unset garante que só atualizaremos o que foi enviado
            update_data = data.model_dump(exclude_unset=True, mode="json")
            for key, value in update_data.items():
                setattr(carousel, key, value)
            
            await self.db.commit()
            await self.db.refresh(carousel)
            
        return carousel

    async def delete_carousel_item(self, item_id: uuid.UUID) -> bool:
        result = await self.db.execute(select(CarouselModel).where(CarouselModel.id == item_id))
        carousel = result.scalars().first()
        
        if carousel:
            await self.db.delete(carousel)
            await self.db.commit()
            return True
            
        return False

    # ==========================================
    # StatCard Operations
    # ==========================================

    async def create_stat_card(self, data: StatCardCreateDTO) -> StatCardModel:
        payload = data.model_dump(mode="json")
        new_stat = StatCardModel(**payload)
        self.db.add(new_stat)
        await self.db.commit()
        await self.db.refresh(new_stat)
        return new_stat

    async def update_stat_card(self, card_id: uuid.UUID, data: StatCardUpdateDTO) -> Optional[StatCardModel]:
        result = await self.db.execute(select(StatCardModel).where(StatCardModel.id == card_id))
        stat = result.scalars().first()
        
        if stat:
            update_data = data.model_dump(exclude_unset=True, mode="json")
            for key, value in update_data.items():
                setattr(stat, key, value)
                
            await self.db.commit()
            await self.db.refresh(stat)
            
        return stat

    async def delete_stat_card(self, card_id: uuid.UUID) -> bool:
        result = await self.db.execute(select(StatCardModel).where(StatCardModel.id == card_id))
        stat = result.scalars().first()
        
        if stat:
            await self.db.delete(stat)
            await self.db.commit()
            return True
            
        return False
