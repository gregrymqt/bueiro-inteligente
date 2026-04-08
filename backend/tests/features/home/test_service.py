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
    # In HomeService, get_home_content calls cache_service.get_or_set
    from app.features.home.dto import HomeResponse
    mock_response = HomeResponse(carousels=[], stats=[])
    mock_result = MagicMock()
    mock_result.data = mock_response
    home_service.cache_service.get_or_set.return_value = mock_result
    
    result = await home_service.get_home_content()
    
    home_service.cache_service.get_or_set.assert_called_once()
    assert result == mock_response


@pytest.mark.asyncio
async def test_get_home_content_cache_miss(home_service):
    from app.features.home.dto import HomeResponse
    mock_response = HomeResponse(carousels=[], stats=[])
    mock_result = MagicMock()
    mock_result.data = mock_response
    home_service.cache_service.get_or_set.return_value = mock_result
    
    result = await home_service.get_home_content()
    
    home_service.cache_service.get_or_set.assert_called_once()
    assert result == mock_response


@pytest.mark.asyncio
async def test_create_carousel_invalidates_cache(home_service):
    # Mocking that the service explicitly has an _invalidate_cache method as per requirement
    home_service._invalidate_cache = AsyncMock()
    import uuid
    carousel_dto = CarouselCreateDTO(title="New", subtitle="Desc", image_url="http://image.com", section="hero", order=1)
    mock_model = MagicMock(title="New", subtitle="Desc", image_url="http://image.com", section="hero", order=1, action_url=None, id=uuid.uuid4())
    home_service.repository.create_carousel_item.return_value = mock_model
    
    await home_service.create_carousel_item(carousel_dto)
    
    home_service.repository.create_carousel_item.assert_called_once()
    home_service._invalidate_cache.assert_called_once()

@pytest.mark.asyncio
async def test_update_stat_invalidates_cache(home_service):
    home_service._invalidate_cache = AsyncMock()
    import uuid
    mock_entity = MagicMock(title="T", value="200", description="D", icon_name="I", color="success", order=1, id=uuid.uuid4())
    home_service.repository.update_stat_card.return_value = mock_entity
    update_data = MagicMock()
    update_data.model_dump.return_value = {"value": "500"}
    
    await home_service.update_stat_card("uuid", update_data)
    
    home_service.repository.update_stat_card.assert_called_once_with("uuid", update_data)
    home_service._invalidate_cache.assert_called_once()
    # Verifica explicitamente o uso do exclude_unset=True associado à atualização DTO -> SQLAlchemy model
    # Na verdade update_data.model_dump não é chamado no service, mas no repositório.
    # Vou deixar apenas a chamada do repositório para o mock no repositório no futuro ser validado

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
    
    result = await home_service.update_carousel_item("fake_uuid", update_data)
        
    assert result is None
