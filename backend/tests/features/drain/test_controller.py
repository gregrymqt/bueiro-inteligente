from contextlib import asynccontextmanager
from datetime import datetime, timezone
from unittest.mock import AsyncMock

import pytest
from fastapi import status
from httpx import ASGITransport, AsyncClient

from app.extensions.auth import get_current_user
from app.extensions.infrastructure import get_db, infrastructure
from app.features.auth.dto import UserTokenData
from app.features.drain.dto import DrainRead
from app.features.drain.service import DrainService
from app.main import app


def build_drain_read(
    *,
    drain_id: int,
    name: str,
    address: str,
    latitude: float,
    longitude: float,
    hardware_id: str,
    is_active: bool,
) -> DrainRead:
    return DrainRead(
        id=drain_id,
        name=name,
        address=address,
        latitude=latitude,
        longitude=longitude,
        hardware_id=hardware_id,
        is_active=is_active,
        created_at=datetime(2026, 4, 11, 12, 0, tzinfo=timezone.utc),
    )


@asynccontextmanager
async def admin_client():
    admin_user = UserTokenData(email="admin@test.com", role="Admin", jti="admin-jti")

    async def override_get_db():
        return AsyncMock()

    original_redis = infrastructure.redis_client
    original_session_factory = infrastructure.session_factory

    app.dependency_overrides[get_current_user] = lambda: admin_user
    app.dependency_overrides[get_db] = override_get_db
    infrastructure.redis_client = AsyncMock()
    infrastructure.session_factory = AsyncMock()

    transport = ASGITransport(app=app)

    try:
        async with AsyncClient(transport=transport, base_url="http://test") as client:
            yield client
    finally:
        app.dependency_overrides.pop(get_current_user, None)
        app.dependency_overrides.pop(get_db, None)
        infrastructure.redis_client = original_redis
        infrastructure.session_factory = original_session_factory


@pytest.mark.asyncio
async def test_list_drains_returns_200_and_items(mocker):
    expected_drain = build_drain_read(
        drain_id=1,
        name="Bueiro Central",
        address="Rua Principal, 100",
        latitude=-23.5505,
        longitude=-46.6333,
        hardware_id="HW-001",
        is_active=True,
    )
    mock_get_all = AsyncMock(return_value=[expected_drain])
    mocker.patch.object(DrainService, "get_all_drains", mock_get_all)

    async with admin_client() as client:
        response = await client.get("/drains", params={"skip": 5, "limit": 10})

    assert response.status_code == status.HTTP_200_OK
    assert response.json() == [expected_drain.model_dump(mode="json")]
    mock_get_all.assert_awaited_once_with(skip=5, limit=10)


@pytest.mark.asyncio
async def test_create_drain_returns_201_created(mocker):
    payload = {
        "name": "Bueiro Novo",
        "address": "Rua Nova, 11",
        "latitude": -23.551,
        "longitude": -46.631,
        "hardware_id": "HW-200",
        "is_active": True,
    }
    expected_drain = build_drain_read(
        drain_id=2,
        name=payload["name"],
        address=payload["address"],
        latitude=payload["latitude"],
        longitude=payload["longitude"],
        hardware_id=payload["hardware_id"],
        is_active=payload["is_active"],
    )
    mock_create = AsyncMock(return_value=expected_drain)
    mocker.patch.object(DrainService, "create_drain", mock_create)

    async with admin_client() as client:
        response = await client.post("/drains", json=payload)

    assert response.status_code == status.HTTP_201_CREATED
    assert response.json() == expected_drain.model_dump(mode="json")
    mock_create.assert_awaited_once()


@pytest.mark.asyncio
async def test_update_drain_returns_200_ok(mocker):
    payload = {
        "name": "Bueiro Atualizado",
        "address": "Rua Atualizada, 200",
        "latitude": -23.552,
        "longitude": -46.632,
        "hardware_id": "HW-300",
        "is_active": False,
    }
    expected_drain = build_drain_read(
        drain_id=3,
        name=payload["name"],
        address=payload["address"],
        latitude=payload["latitude"],
        longitude=payload["longitude"],
        hardware_id=payload["hardware_id"],
        is_active=payload["is_active"],
    )
    mock_update = AsyncMock(return_value=expected_drain)
    mocker.patch.object(DrainService, "update_drain", mock_update)

    async with admin_client() as client:
        response = await client.put("/drains/3", json=payload)

    assert response.status_code == status.HTTP_200_OK
    assert response.json() == expected_drain.model_dump(mode="json")
    mock_update.assert_awaited_once()


@pytest.mark.asyncio
async def test_delete_drain_returns_204_no_content(mocker):
    mock_delete = AsyncMock(return_value=None)
    mocker.patch.object(DrainService, "delete_drain", mock_delete)

    async with admin_client() as client:
        response = await client.delete("/drains/4")

    assert response.status_code == status.HTTP_204_NO_CONTENT
    assert response.content == b""
    mock_delete.assert_awaited_once_with(4)