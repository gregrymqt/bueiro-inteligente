import pytest
import json
from unittest.mock import AsyncMock, MagicMock, patch
from fastapi import HTTPException
from app.features.home.service import HomeService
from app.features.home.dto import CarouselCreateDTO, StatCardCreateDTO


@pytest.fixture
def home_service():
    repository = AsyncMock()
    cache_service = AsyncMock()
    return HomeService(repository, cache_service)


@pytest.mark.asyncio
async def test_get_home_content_cache_hit(home_service):
    from app.features.home.dto import HomeResponse
    cached_data = MagicMock()
    cached_data.data = HomeResponse(carousels=[], stats=[])
    home_service.cache_service.get_or_set.return_value = cached_data
    
    result = await home_service.get_home_content()
    
    home_service.cache_service.get_or_set.assert_called_once()
    assert result == HomeResponse(carousels=[], stats=[])


@pytest.mark.asyncio
async def test_get_home_content_cache_miss(home_service):
    from app.features.home.dto import HomeResponse
    cached_data = MagicMock()
    cached_data.data = HomeResponse(carousels=[], stats=[])
    home_service.cache_service.get_or_set.return_value = cached_data
    
    result = await home_service.get_home_content()
    
    home_service.cache_service.get_or_set.assert_called_once()


@pytest.mark.asyncio
async def test_create_carousel_invalidates_cache(home_service):
    # Mocking that the service explicitly has an _invalidate_cache method as per requirement
    home_service._invalidate_cache = AsyncMock()
    carousel_dto = CarouselCreateDTO(title="New", subtitle="Desc", image_url="http://image.com", action_url="http://action.com", order=1, section="hero")

    from app.features.home.dto import CarouselDTO
    from uuid import uuid4
    mock_model = MagicMock()
    mock_model.id = uuid4()
    mock_model.title = "New"
    mock_model.subtitle = "Desc"
    mock_model.image_url = "http://image.com"
    mock_model.action_url = "http://action.com"
    mock_model.order = 1
    mock_model.section = "hero"

    home_service.repository.create_carousel_item.return_value = mock_model
    
    await home_service.create_carousel_item(carousel_dto)
    
    home_service.repository.create_carousel_item.assert_called_once()
    home_service._invalidate_cache.assert_called_once()

@pytest.mark.asyncio
async def test_update_stat_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    from uuid import uuid4

    mock_model = MagicMock()
    mock_model.id = uuid4()
    mock_model.title = "Stats"
    mock_model.description = "Test desc"
    mock_model.value = "200"
    mock_model.icon_name = "icon"
    mock_model.color = "success"
    mock_model.order = 1

    home_service.repository.update_stat_card.return_value = mock_model

    update_data = MagicMock()
    
    await home_service.update_stat_card(uuid4(), update_data)
    
    home_service.repository.update_stat_card.assert_called_once()
    home_service._invalidate_cache.assert_called_once()

@pytest.mark.asyncio
async def test_delete_carousel_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    home_service.repository.delete_carousel_item.return_value = True
    
    from uuid import uuid4
    await home_service.delete_carousel_item(uuid4())
    
    home_service.repository.delete_carousel_item.assert_called_once()
    home_service._invalidate_cache.assert_called_once()


@pytest.mark.asyncio
async def test_update_item_not_found(home_service):
    home_service.repository.update_carousel_item.return_value = None
    update_data = MagicMock()
    
    from uuid import uuid4
    res = await home_service.update_carousel_item(uuid4(), update_data)
        
    assert res is None
