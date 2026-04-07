from fastapi import APIRouter, Depends, HTTPException, status
from typing import List, Optional
import uuid

from app.extensions.infrastructure import get_db
from sqlalchemy.ext.asyncio import AsyncSession

from app.extensions.auth import RoleChecker
from app.features.auth.dto import UserTokenData

from app.features.cache.service import RedisCacheService
from app.extensions.infrastructure import infrastructure
from app.core.security import RateLimiter

from app.features.home.dto import (
    HomeResponse,
    CarouselDTO, CarouselCreateDTO, CarouselUpdateDTO,
    StatCardDTO, StatCardCreateDTO, StatCardUpdateDTO
)
from app.features.home.repository import HomeRepository
from app.features.home.service import HomeService

router = APIRouter(prefix="/home", tags=["Home / Dashboard"])

# ==========================================
# Injeção de Dependência (Factory)
# ==========================================

def get_home_service(db: AsyncSession = Depends(get_db)) -> HomeService:
    repository = HomeRepository(db)
    # Reusa a infraestrutura de Redis existente
    cache_service = RedisCacheService(infrastructure.redis_client) 
    return HomeService(repository, cache_service)

# ==========================================
# Endpoints Públicos
# ==========================================

@router.get("", response_model=HomeResponse, dependencies=[Depends(RateLimiter(times=10, seconds=10))])
async def get_home_page(service: HomeService = Depends(get_home_service)):
    """
    Retorna o conteúdo público da página home: Carousels (Hero, alertas)
    e Stats atualizados em cache.
    """
    return await service.get_home_content()

# ==========================================
# Endpoints de Carousel (Somente Admin)
# ==========================================

@router.post(
    "/carousel", 
    response_model=CarouselDTO, 
    status_code=status.HTTP_201_CREATED,
    dependencies=[Depends(RoleChecker(['Admin'])), Depends(RateLimiter(times=5, seconds=10))]
)
async def create_carousel_item(
    payload: CarouselCreateDTO,
    service: HomeService = Depends(get_home_service)
):
    """
    [ADMIN] Cria uma nova imagem/alerta para o Carousel da Home.
    O cache das informações de Home será limpo automaticamente.
    """
    return await service.create_carousel_item(payload)

@router.patch(
    "/carousel/{item_id}", 
    response_model=CarouselDTO,
    dependencies=[Depends(RoleChecker(['Admin'])), Depends(RateLimiter(times=5, seconds=10))]
)
async def update_carousel_item(
    item_id: uuid.UUID,
    payload: CarouselUpdateDTO,
    service: HomeService = Depends(get_home_service)
):
    """
    [ADMIN] Atualiza os campos de um Carousel item de forma parcial.
    """
    updated_item = await service.update_carousel_item(item_id, payload)
    if not updated_item:
        raise HTTPException(status_code=404, detail="Item do carousel não encontrado.")
    return updated_item

@router.delete(
    "/carousel/{item_id}", 
    status_code=status.HTTP_204_NO_CONTENT,
    dependencies=[Depends(RoleChecker(['Admin'])), Depends(RateLimiter(times=5, seconds=10))]
)
async def delete_carousel_item(
    item_id: uuid.UUID,
    service: HomeService = Depends(get_home_service)
):
    """
    [ADMIN] Remove um item do Carousel da Home.
    """
    success = await service.delete_carousel_item(item_id)
    if not success:
        raise HTTPException(status_code=404, detail="Item do carousel não encontrado.")
    return None

# ==========================================
# Endpoints de Stat Cards (Somente Admin)
# ==========================================

@router.post(
    "/stats", 
    response_model=StatCardDTO, 
    status_code=status.HTTP_201_CREATED,
    dependencies=[Depends(RoleChecker(['Admin'])), Depends(RateLimiter(times=5, seconds=10))]
)
async def create_stat_card(
    payload: StatCardCreateDTO,
    service: HomeService = Depends(get_home_service)
):
    """
    [ADMIN] Cria um novo Card Estatístico para a Home.
    """
    return await service.create_stat_card(payload)

@router.patch(
    "/stats/{card_id}", 
    response_model=StatCardDTO,
    dependencies=[Depends(RoleChecker(['Admin'])), Depends(RateLimiter(times=5, seconds=10))]
)
async def update_stat_card(
    card_id: uuid.UUID,
    payload: StatCardUpdateDTO,
    service: HomeService = Depends(get_home_service)
):
    """
    [ADMIN] Atualiza os campos de um Card de forma parcial.
    """
    updated_card = await service.update_stat_card(card_id, payload)
    if not updated_card:
        raise HTTPException(status_code=404, detail="Stat card não encontrado.")
    return updated_card

@router.delete(
    "/stats/{card_id}", 
    status_code=status.HTTP_204_NO_CONTENT,
    dependencies=[Depends(RoleChecker(['Admin'])), Depends(RateLimiter(times=5, seconds=10))]
)
async def delete_stat_card(
    card_id: uuid.UUID,
    service: HomeService = Depends(get_home_service)
):
    """
    [ADMIN] Remove um Card Estatístico da Home.
    """
    success = await service.delete_stat_card(card_id)
    if not success:
        raise HTTPException(status_code=404, detail="Stat card não encontrado.")
    return None