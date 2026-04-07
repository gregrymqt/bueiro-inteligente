import pytest
from unittest.mock import AsyncMock, MagicMock
from app.features.monitoring.services.bueiro_service import BueiroService
from app.features.monitoring.dto import SensorPayloadDTO, DrainStatusDTO
from app.features.monitoring.interfaces import IDrainRepository
from app.features.cache.service import RedisCacheService
from app.features.monitoring.services.broadcast_service import BroadcastService
from datetime import datetime, timezone

@pytest.fixture
def mock_repository():
    repo = AsyncMock(spec=IDrainRepository)
    repo.save_sensor_data = AsyncMock()
    repo.get_latest_status = AsyncMock()
    return repo

@pytest.fixture
def mock_cache_service():
    cache = AsyncMock(spec=RedisCacheService)
    return cache

@pytest.fixture
def mock_broadcast_service():
    broadcast = AsyncMock(spec=BroadcastService)
    broadcast.enviar_atualizacao_bueiro = AsyncMock()
    return broadcast

@pytest.mark.asyncio
async def test_process_sensor_data_critical(mock_repository, mock_cache_service, mock_broadcast_service):
    service = BueiroService(repository=mock_repository, cache_service=mock_cache_service, broadcast_service=mock_broadcast_service)

    # Critical level: obstruction >= 80%. Max depth is 120cm.
    # 80% of 120 = 96cm obstructed. Distance read = 24cm.
    payload = SensorPayloadDTO(
        id_bueiro="bueiro-01",
        distancia_cm=20.0, # 100cm obstructed, >80% -> Critico
        latitude=-23.0,
        longitude=-46.0
    )

    # Mock fallback db response
    status_db = DrainStatusDTO(
        id_bueiro="bueiro-01",
        distancia_cm=20.0,
        nivel_obstrucao=83.33,
        status="Crítico",
        latitude=-23.0,
        longitude=-46.0,
        ultima_atualizacao=datetime.now(timezone.utc)
    )
    mock_repository.get_latest_status.return_value = status_db

    result = await service.process_sensor_data(payload)

    assert result.status == "Crítico"
    assert result.nivel_obstrucao > 80.0

    mock_repository.save_sensor_data.assert_awaited_once()
    mock_broadcast_service.enviar_atualizacao_bueiro.assert_awaited_once_with(status_db)

@pytest.mark.asyncio
async def test_process_sensor_data_alert(mock_repository, mock_cache_service, mock_broadcast_service):
    service = BueiroService(repository=mock_repository, cache_service=mock_cache_service, broadcast_service=mock_broadcast_service)

    # Alert level: obstruction >= 50% and < 80%.
    # 50% of 120 = 60cm obstructed. Distance read = 60cm.
    # 60% of 120 = 72cm obstructed. Distance read = 48cm.
    payload = SensorPayloadDTO(
        id_bueiro="bueiro-01",
        distancia_cm=50.0, # 70cm obstructed, ~58% -> Alerta
        latitude=-23.0,
        longitude=-46.0
    )

    status_db = DrainStatusDTO(
        id_bueiro="bueiro-01",
        distancia_cm=50.0,
        nivel_obstrucao=58.33,
        status="Alerta",
        latitude=-23.0,
        longitude=-46.0,
        ultima_atualizacao=datetime.now(timezone.utc)
    )
    mock_repository.get_latest_status.return_value = status_db

    result = await service.process_sensor_data(payload)

    assert result.status == "Alerta"
    assert 50.0 <= result.nivel_obstrucao < 80.0

    mock_broadcast_service.enviar_atualizacao_bueiro.assert_awaited_once_with(status_db)

@pytest.mark.asyncio
async def test_process_sensor_data_normal(mock_repository, mock_cache_service, mock_broadcast_service):
    service = BueiroService(repository=mock_repository, cache_service=mock_cache_service, broadcast_service=mock_broadcast_service)

    # Normal level: obstruction < 50%.
    payload = SensorPayloadDTO(
        id_bueiro="bueiro-01",
        distancia_cm=100.0, # 20cm obstructed, ~16% -> Normal
        latitude=-23.0,
        longitude=-46.0
    )

    status_db = DrainStatusDTO(
        id_bueiro="bueiro-01",
        distancia_cm=100.0,
        nivel_obstrucao=16.67,
        status="Normal",
        latitude=-23.0,
        longitude=-46.0,
        ultima_atualizacao=datetime.now(timezone.utc)
    )
    mock_repository.get_latest_status.return_value = status_db

    result = await service.process_sensor_data(payload)

    assert result.status == "Normal"
    assert result.nivel_obstrucao < 50.0

    # It still broadcasts if normal but found in DB according to the code (unless BroadcastService logic filters it internally)
    mock_broadcast_service.enviar_atualizacao_bueiro.assert_awaited_once_with(status_db)
