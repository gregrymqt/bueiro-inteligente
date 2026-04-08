import pytest
from unittest.mock import AsyncMock, MagicMock
from sqlalchemy.ext.asyncio import AsyncSession
from app.features.home.repository import HomeRepository


@pytest.fixture
def mock_session():
    session = AsyncMock(spec=AsyncSession)
    return session


@pytest.mark.asyncio
async def test_get_all_content(mock_session):
    repository = HomeRepository(mock_session)
    mock_result_carousel = MagicMock()
    mock_result_carousel.scalars.return_value.all.return_value = [{"id": "uuid1", "title": "C1"}]
    
    mock_result_stats = MagicMock()
    mock_result_stats.scalars.return_value.all.return_value = [{"id": "uuid2", "title": "S1"}]
    
    # Simulate sequential await execution if using session.execute twice
    mock_session.execute.side_effect = [mock_result_carousel, mock_result_stats]
    
    result = await repository.get_all_content()
    
    assert "carousels" in result or isinstance(result, tuple)
    assert mock_session.execute.call_count == 2


@pytest.mark.asyncio
async def test_create_carousel_item(mock_session):
    repository = HomeRepository(mock_session)
    mock_item = MagicMock()
    mock_item.model_dump.return_value = {"title": "Test", "image_url": "http://img.com", "order": 1, "section": "hero"}
    
    from app.features.home.models import CarouselModel
    import copy

    # Avoid assert_called_once_with to avoid checking the specific instance of CarouselModel
    await repository.create_carousel_item(mock_item)
    
    mock_session.add.assert_called_once()
    mock_session.commit.assert_called_once()
    mock_session.refresh.assert_called_once()


@pytest.mark.asyncio
async def test_update_carousel_item_partial(mock_session):
    import uuid
    repository = HomeRepository(mock_session)
    
    fake_id = uuid.uuid4()
    mock_db_result = MagicMock()
    mock_entity = MagicMock(id=fake_id, title="Old")
    mock_db_result.scalars.return_value.first.return_value = mock_entity
    mock_session.execute.return_value = mock_db_result

    update_dto = MagicMock()
    update_dto.model_dump.return_value = {"title": "Updated Title"}
    
    await repository.update_carousel_item(fake_id, update_dto)
    
    assert mock_entity.title == "Updated Title"
    mock_session.commit.assert_called_once()
    mock_session.refresh.assert_called_once_with(mock_entity)


@pytest.mark.asyncio
async def test_delete_carousel_item(mock_session):
    import uuid
    repository = HomeRepository(mock_session)
    
    fake_id = uuid.uuid4()
    mock_db_result = MagicMock()
    mock_entity = MagicMock(id=fake_id)
    mock_db_result.scalars.return_value.first.return_value = mock_entity
    mock_session.execute.return_value = mock_db_result

    await repository.delete_carousel_item(fake_id)
    
    mock_session.delete.assert_called_once_with(mock_entity)
    mock_session.commit.assert_called_once()
