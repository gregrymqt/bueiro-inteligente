import pytest
from httpx import AsyncClient
from sqlalchemy import select

from app.features.monitoring.models import DrainStatus


@pytest.mark.asyncio
async def test_integracao_recebimento_dados_hardware(test_client: AsyncClient, db_session, monkeypatch):
    from app.core.config import settings
    from app.extensions.infrastructure import get_db, infrastructure
    from app.main import app

    hardware_token = "secret_hardware_key"
    monkeypatch.setattr(settings, "HARDWARE_TOKEN", hardware_token)

    original_redis_client = infrastructure.redis_client
    infrastructure.redis_client = None

    async def override_get_db():
        yield db_session

    app.dependency_overrides[get_db] = override_get_db

    payload = {
        "id_bueiro": "esp32-teste-integracao",
        "distancia_cm": 15.0,
        "latitude": -23.5505,
        "longitude": -46.6333,
    }

    headers = {
        "Authorization": f"Bearer {hardware_token}",
    }

    try:
        response = await test_client.post("/monitoring/medicoes", json=payload, headers=headers)

        assert response.status_code == 200, response.text
        body = response.json()
        assert body["id_bueiro"] == "esp32-teste-integracao"
        assert body["status"] == "Crítico"
        assert body["nivel_obstrucao"] == 87.5

        result = await db_session.execute(
            select(DrainStatus).where(DrainStatus.id_bueiro == "esp32-teste-integracao")
        )
        saved_row = result.scalar_one_or_none()

        assert saved_row is not None
        assert saved_row.id_bueiro == "esp32-teste-integracao"
        assert saved_row.distancia_cm == 15.0
        assert saved_row.status == "Crítico"
        assert saved_row.sincronizado_rows is False or saved_row.sincronizado_rows == 0
    finally:
        app.dependency_overrides.pop(get_db, None)
        infrastructure.redis_client = original_redis_client
