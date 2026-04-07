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
    
    await repository.create_carousel_item(mock_item)
    
    mock_session.add.assert_called_once_with(mock_item)
    mock_session.commit.assert_called_once()
    mock_session.refresh.assert_called_once_with(mock_item)


@pytest.mark.asyncio
async def test_update_carousel_item_partial(mock_session):
    repository = HomeRepository(mock_session)
    mock_entity = MagicMock()
    
    # Simulating the dictionary dumped with exclude_unset=True
    update_data = {"title": "Updated Title"}
    
    for key, value in update_data.items():
        setattr(mock_entity, key, value)
        
    await repository.update_carousel_item(mock_entity)
    
    assert mock_entity.title == "Updated Title"
    mock_session.commit.assert_called_once()
    mock_session.refresh.assert_called_once_with(mock_entity)


@pytest.mark.asyncio
async def test_delete_carousel_item(mock_session):
    repository = HomeRepository(mock_session)
    mock_entity = MagicMock()
    
    await repository.delete_carousel_item(mock_entity)
    
    mock_session.delete.assert_called_once_with(mock_entity)
    mock_session.commit.assert_called_once()
