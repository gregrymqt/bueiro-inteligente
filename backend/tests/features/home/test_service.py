import pytest
import json
from unittest.mock import AsyncMock, MagicMock, patch
from fastapi import HTTPException
from app.features.home.service import HomeService
from app.features.home.dto import CarouselCreateDTO, StatCardCreateDTO


@pytest.fixture
def home_service():
    repository = AsyncMock()
    cache = AsyncMock()
    return HomeService(repository, cache)


@pytest.mark.asyncio
async def test_get_home_content_cache_hit(home_service):
    cached_data = json.dumps({"carousels": [], "stats": []})
    home_service.cache.get.return_value = cached_data
    
    result = await home_service.get_home_content()
    
    home_service.cache.get.assert_called_once_with("home_content:all")
    assert result == {"carousels": [], "stats": []}
    home_service.repository.get_all_content.assert_not_called()


@pytest.mark.asyncio
async def test_get_home_content_cache_miss(home_service):
    home_service.cache.get.return_value = None
    mock_items = {"carousels": [{"id": "1", "title": "A"}], "stats": [{"id": "2", "value": "100"}]}
    home_service.repository.get_all_content.return_value = mock_items
    
    result = await home_service.get_home_content()
    
    home_service.cache.get.assert_called_once_with("home_content:all")
    home_service.repository.get_all_content.assert_called_once()
    home_service.cache.set.assert_called_once()
    assert "carousels" in result
    assert "stats" in result


@pytest.mark.asyncio
async def test_create_carousel_invalidates_cache(home_service):
    # Mocking that the service explicitly has an _invalidate_cache method as per requirement
    home_service._invalidate_cache = AsyncMock()
    carousel_dto = CarouselCreateDTO(title="New", description="Desc", image_url="http://image.com")
    home_service.repository.create_carousel_item.return_value = MagicMock(id="new_uuid", title="New")
    
    await home_service.create_carousel_item(carousel_dto)
    
    home_service.repository.create_carousel_item.assert_called_once()
    home_service._invalidate_cache.assert_called_once()

@pytest.mark.asyncio
async def test_update_stat_card_item_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    mock_entity = MagicMock(id="uuid", key="users", value="200")
    home_service.repository.get_stat_card_item_by_id.return_value = mock_entity
    update_data = MagicMock()
    update_data.model_dump.return_value = {"value": "500"}
    
    await home_service.update_stat_card_item("uuid", update_data)
    
    home_service.repository.update_stat_card_item.assert_called_once()
    home_service._invalidate_cache.assert_called_once()
    # Verifica explicitamente o uso do exclude_unset=True associado à atualização DTO -> SQLAlchemy model
    update_data.model_dump.assert_called_once_with(exclude_unset=True)

@pytest.mark.asyncio
async def test_delete_carousel_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    mock_entity = MagicMock(id="uuid")
    home_service.repository.get_carousel_item_by_id.return_value = mock_entity
    
    await home_service.delete_carousel_item("uuid")
    
    home_service.repository.delete_carousel_item.assert_called_once_with(mock_entity)
    home_service._invalidate_cache.assert_called_once()


@pytest.mark.asyncio
async def test_update_item_not_found(home_service):
    home_service.repository.get_carousel_item_by_id.return_value = None
    update_data = MagicMock()
    
    with pytest.raises(HTTPException) as exc_info:
        await home_service.update_carousel_item("fake_uuid", update_data)
        
    assert exc_info.value.status_code == 404
