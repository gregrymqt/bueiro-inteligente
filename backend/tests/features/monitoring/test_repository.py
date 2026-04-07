import pytest
from unittest.mock import AsyncMock, MagicMock
from app.features.monitoring.repository import DrainRepository
from app.features.monitoring.dto import DrainStatusDTO
from app.features.cache.interfaces import ICacheService
from sqlalchemy.ext.asyncio import AsyncSession
from datetime import datetime, timezone

@pytest.fixture
def mock_db_session():
    session = AsyncMock(spec=AsyncSession)
    session.add = MagicMock()
    session.commit = AsyncMock()
    session.refresh = AsyncMock()
    session.rollback = AsyncMock()
    session.execute = AsyncMock()
    return session

@pytest.fixture
def mock_cache_service():
    cache = AsyncMock(spec=ICacheService)
    cache.set = AsyncMock()
    return cache

@pytest.mark.asyncio
async def test_save_sensor_data(mock_db_session, mock_cache_service):
    repo = DrainRepository(db_client=mock_db_session, cache_service=mock_cache_service)

    data = DrainStatusDTO(
        id_bueiro="bueiro-01",
        distancia_cm=10.0,
        nivel_obstrucao=50.0,
        status="Alerta",
        latitude=-23.0,
        longitude=-46.0,
        ultima_atualizacao=datetime.now(timezone.utc)
    )

    await repo.save_sensor_data(data)

    # Verify cache call
    mock_cache_service.set.assert_awaited_once()

    # Verify DB calls
    mock_db_session.add.assert_called_once()
    mock_db_session.commit.assert_awaited_once()
    mock_db_session.refresh.assert_awaited_once()

@pytest.mark.asyncio
async def test_save_sensor_data_db_error(mock_db_session, mock_cache_service):
    repo = DrainRepository(db_client=mock_db_session, cache_service=mock_cache_service)

    data = DrainStatusDTO(
        id_bueiro="bueiro-01",
        distancia_cm=10.0,
        nivel_obstrucao=50.0,
        status="Alerta",
        latitude=-23.0,
        longitude=-46.0,
        ultima_atualizacao=datetime.now(timezone.utc)
    )

    # Simulate DB error
    mock_db_session.commit.side_effect = Exception("DB Error")

    with pytest.raises(Exception, match="Falha ao salvar medição no banco de dados"):
        await repo.save_sensor_data(data)

    mock_db_session.rollback.assert_awaited_once()
