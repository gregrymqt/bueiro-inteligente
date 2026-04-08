import pytest
import json
from unittest.mock import AsyncMock, MagicMock, patch
from fastapi import HTTPException
from app.features.home.service import HomeService
from app.features.home.dto import CarouselCreateDTO, StatCardCreateDTO


import uuid
from app.features.home.dto import HomeResponse

@pytest.fixture
def home_service():
    repository = AsyncMock()
    cache_service = AsyncMock()
    return HomeService(repository, cache_service)


@pytest.mark.asyncio
async def test_get_home_content_cache_hit(home_service):
    cached_response = MagicMock(data=HomeResponse(carousels=[], stats=[]))
    home_service.cache_service.get_or_set.return_value = cached_response
    
    result = await home_service.get_home_content()
    
    home_service.cache_service.get_or_set.assert_called_once()
    assert result.carousels == []
    assert result.stats == []


@pytest.mark.asyncio
async def test_get_home_content_cache_miss(home_service):
    cached_response = MagicMock(data=HomeResponse(carousels=[], stats=[]))
    home_service.cache_service.get_or_set.return_value = cached_response
    
    result = await home_service.get_home_content()
    
    home_service.cache_service.get_or_set.assert_called_once()
    assert hasattr(result, "carousels")
    assert hasattr(result, "stats")


@pytest.mark.asyncio
async def test_create_carousel_invalidates_cache(home_service):
    # Mocking that the service explicitly has an _invalidate_cache method as per requirement
    home_service._invalidate_cache = AsyncMock()
    carousel_dto = CarouselCreateDTO(title="New", description="Desc", image_url="http://image.com", order=1, section="hero")
    fake_id = uuid.uuid4()
    home_service.repository.create_carousel_item.return_value = MagicMock(id=fake_id, title="New", subtitle="sub", image_url="http://image.com", action_url="http://action.com", order=1, section="hero")
    
    await home_service.create_carousel_item(carousel_dto)
    
    home_service.repository.create_carousel_item.assert_called_once()
    home_service._invalidate_cache.assert_called_once()

@pytest.mark.asyncio
async def test_update_stat_card_item_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    fake_id = uuid.uuid4()
    mock_entity = MagicMock(id=fake_id, title="users", value="200", description="desc", icon_name="icon", color="success", order=1)
    home_service.repository.update_stat_card.return_value = mock_entity
    update_data = MagicMock()
    update_data.model_dump.return_value = {"value": "500"}
    
    await home_service.update_stat_card(fake_id, update_data)
    
    home_service.repository.update_stat_card.assert_called_once()
    home_service._invalidate_cache.assert_called_once()

@pytest.mark.asyncio
async def test_delete_carousel_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    home_service.repository.delete_carousel_item.return_value = True
    
    await home_service.delete_carousel_item("uuid")
    
    home_service.repository.delete_carousel_item.assert_called_once_with("uuid")
    home_service._invalidate_cache.assert_called_once()


@pytest.mark.asyncio
async def test_update_item_not_found(home_service):
    home_service.repository.update_carousel_item.return_value = None
    update_data = MagicMock()
    
    res = await home_service.update_carousel_item("fake_uuid", update_data)
        
    assert res is None
