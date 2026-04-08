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
    from app.features.home.models import CarouselModel
    repository = HomeRepository(mock_session)
    mock_item = MagicMock()
    
    await repository.create_carousel_item(mock_item)
    
    # Verify that add was called with a CarouselModel instance
    assert mock_session.add.call_count == 1
    args, _ = mock_session.add.call_args
    assert isinstance(args[0], CarouselModel)

    mock_session.commit.assert_called_once()
    assert mock_session.refresh.call_count == 1


@pytest.mark.asyncio
async def test_update_carousel_item_partial(mock_session):
    repository = HomeRepository(mock_session)
    mock_id = "123e4567-e89b-12d3-a456-426614174000"

    mock_model = MagicMock()
    mock_result = MagicMock()
    mock_result.scalars.return_value.first.return_value = mock_model
    mock_session.execute.return_value = mock_result
    
    mock_data = MagicMock()
    # Simulating the dictionary dumped with exclude_unset=True
    update_data = {"title": "Updated Title"}
    mock_data.model_dump.return_value = update_data
        
    await repository.update_carousel_item(mock_id, mock_data)
    
    assert mock_model.title == "Updated Title"
    mock_session.commit.assert_called_once()
    mock_session.refresh.assert_called_once_with(mock_model)


@pytest.mark.asyncio
async def test_delete_carousel_item(mock_session):
    repository = HomeRepository(mock_session)
    mock_id = "123e4567-e89b-12d3-a456-426614174000"

    mock_model = MagicMock()
    mock_result = MagicMock()
    mock_result.scalars.return_value.first.return_value = mock_model
    mock_session.execute.return_value = mock_result
    
    await repository.delete_carousel_item(mock_id)
    
    mock_session.delete.assert_called_once_with(mock_model)
    mock_session.commit.assert_called_once()
