import pytest
from httpx import AsyncClient

@pytest.mark.asyncio
async def test_medicoes_endpoint(test_client: AsyncClient, mocker):
    # Mocking the service injection
    from app.features.monitoring.services.bueiro_service import BueiroService
    from app.features.monitoring.dto import DrainStatusDTO
    from datetime import datetime, timezone

    mock_service = mocker.AsyncMock(spec=BueiroService)

    expected_result = DrainStatusDTO(
        id_bueiro="bueiro-01",
        distancia_cm=20.0,
        nivel_obstrucao=83.33,
        status="Crítico",
        latitude=-23.0,
        longitude=-46.0,
        ultima_atualizacao=datetime.now(timezone.utc)
    )

    mock_service.process_sensor_data.return_value = expected_result

    # Override dependency just for this test
    from app.main import app
    from app.features.monitoring.controller import get_monitoring_service

    app.dependency_overrides[get_monitoring_service] = lambda: mock_service

    payload = {
        "id_bueiro": "bueiro-01",
        "distancia_cm": 20.0,
        "latitude": -23.0,
        "longitude": -46.0
    }

    headers = {"Authorization": "Bearer mocked_token", "X-Hardware-Token": "mocked_hardware_token"}

    # We also need to mock `verify_hardware_token` and `RateLimiter`
    from app.extensions.auth import verify_hardware_token
    from app.core.security import RateLimiter

    async def mock_verify():
        return True

    app.dependency_overrides[verify_hardware_token] = mock_verify

    # RateLimiter is a class-based dependency, it can be tricky to override the exact instance
    # To simplify, we override it if needed, or if we mock redis correctly it won't fail.
    # Since we mocked redis_client in conftest, RateLimiter will use it.

    response = await test_client.post("/monitoring/medicoes", json=payload, headers=headers)

    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "Crítico"
    assert data["id_bueiro"] == "bueiro-01"

    app.dependency_overrides.pop(get_monitoring_service, None)
    app.dependency_overrides.pop(verify_hardware_token, None)

@pytest.mark.asyncio
async def test_medicoes_endpoint_invalid_data(test_client: AsyncClient):
    # Envia string em vez de float para causar erro 422
    payload = {
        "id_bueiro": "bueiro-01",
        "distancia_cm": "invalid_string",
        "latitude": -23.0,
        "longitude": -46.0
    }

    # Vamos manter o mock de auth para isolar o erro de DTO
    from app.main import app
    from app.extensions.auth import verify_hardware_token
    async def mock_verify():
        return True
    app.dependency_overrides[verify_hardware_token] = mock_verify

    response = await test_client.post("/monitoring/medicoes", json=payload)

    assert response.status_code == 422

    app.dependency_overrides.pop(verify_hardware_token, None)

@pytest.mark.asyncio
async def test_medicoes_endpoint_unauthorized(test_client: AsyncClient):
    # Envia payload válido, mas com auth falhando
    payload = {
        "id_bueiro": "bueiro-01",
        "distancia_cm": 20.0,
        "latitude": -23.0,
        "longitude": -46.0
    }

    # Aqui não substituímos o verify_hardware_token, deixamos a implementação real falhar
    # Pois não passaremos token válido

    response = await test_client.post("/monitoring/medicoes", json=payload)

    # Sem token -> 401 Unauthorized
    assert response.status_code == 401
