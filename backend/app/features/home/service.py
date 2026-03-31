from typing import Optional
import uuid

from app.features.home.repository import HomeRepository
from app.features.cache.interfaces import ICacheService
from app.features.home.dto import (
    HomeResponse,
    CarouselDTO, CarouselCreateDTO, CarouselUpdateDTO,
    StatCardDTO, StatCardCreateDTO, StatCardUpdateDTO
)

class HomeService:
    HOME_CACHE_KEY = "home_content:all"
    HOME_CACHE_TTL = 3600  # 1 hora em segundos

    def __init__(self, repository: HomeRepository, cache_service: ICacheService):
        self.repository = repository
        self.cache_service = cache_service

    async def _invalidate_cache(self) -> None:
        """Invalida o cache da página Home para que as atualizações sejam refletidas."""
        await self.cache_service.delete(self.HOME_CACHE_KEY)

    async def _fetch_home_content_from_db(self) -> HomeResponse:
        """Busca do banco e mapeia as entidades SQLAlchemy para DTOs do Pydantic."""
        data = await self.repository.get_all_content()
        
        carousels = [CarouselDTO.model_validate(c) for c in data.get("carousels", [])]
        stats = [StatCardDTO.model_validate(s) for s in data.get("stats", [])]
        
        return HomeResponse(carousels=carousels, stats=stats)

    async def get_home_content(self) -> HomeResponse:
        """
        Retorna o conteúdo da Home, utilizando estratégia de Cache-Aside (Redis).
        Se não estiver no cache, busca no banco e salva por 1 hora.
        """
        response_dto = await self.cache_service.get_or_set(
            key=self.HOME_CACHE_KEY,
            fetch_func=self._fetch_home_content_from_db,
            model_type=HomeResponse,
            ttl_seconds=self.HOME_CACHE_TTL
        )
        return response_dto.data

    # ==========================================
    # Carousel Operations
    # ==========================================

    async def create_carousel_item(self, data: CarouselCreateDTO) -> CarouselDTO:
        model = await self.repository.create_carousel_item(data)
        
        # Invalida o cache após mutação
        await self._invalidate_cache()
        
        return CarouselDTO.model_validate(model)

    async def update_carousel_item(self, item_id: uuid.UUID, data: CarouselUpdateDTO) -> Optional[CarouselDTO]:
        model = await self.repository.update_carousel_item(item_id, data)
        if not model:
            return None
            
        # Invalida o cache após mutação
        await self._invalidate_cache()
        
        return CarouselDTO.model_validate(model)

    async def delete_carousel_item(self, item_id: uuid.UUID) -> bool:
        success = await self.repository.delete_carousel_item(item_id)
        if success:
            await self._invalidate_cache()
        return success

    # ==========================================
    # StatCard Operations
    # ==========================================

    async def create_stat_card(self, data: StatCardCreateDTO) -> StatCardDTO:
        model = await self.repository.create_stat_card(data)
        
        await self._invalidate_cache()
        
        return StatCardDTO.model_validate(model)

    async def update_stat_card(self, card_id: uuid.UUID, data: StatCardUpdateDTO) -> Optional[StatCardDTO]:
        model = await self.repository.update_stat_card(card_id, data)
        if not model:
            return None
            
        await self._invalidate_cache()
        
        return StatCardDTO.model_validate(model)

    async def delete_stat_card(self, card_id: uuid.UUID) -> bool:
        success = await self.repository.delete_stat_card(card_id)
        if success:
            await self._invalidate_cache()
        return success
