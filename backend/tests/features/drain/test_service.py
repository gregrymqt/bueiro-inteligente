from datetime import datetime, timezone
from types import SimpleNamespace
from unittest.mock import AsyncMock

import pytest
from fastapi import HTTPException

from app.features.drain.dto import DrainCreate, DrainRead, DrainUpdate
from app.features.drain.interfaces import IDrainRepository
from app.features.drain.service import DrainService


def build_drain_entity(
    *,
    drain_id: int,
    name: str = "Bueiro Central",
    address: str = "Rua Principal, 100",
    latitude: float = -23.5505,
    longitude: float = -46.6333,
    hardware_id: str = "HW-001",
    is_active: bool = True,
) -> SimpleNamespace:
    return SimpleNamespace(
        id=drain_id,
        name=name,
        address=address,
        latitude=latitude,
        longitude=longitude,
        is_active=is_active,
        hardware_id=hardware_id,
        created_at=datetime(2026, 4, 11, 12, 0, tzinfo=timezone.utc),
    )


@pytest.fixture
def repository() -> AsyncMock:
    return AsyncMock(spec=IDrainRepository)


@pytest.mark.asyncio
async def test_create_drain_success(repository: AsyncMock):
    service = DrainService(repository)
    payload = DrainCreate(
        name="Bueiro Norte",
        address="Avenida Norte, 10",
        latitude=-23.1,
        longitude=-46.1,
        hardware_id="HW-002",
        is_active=True,
    )
    created_entity = build_drain_entity(
        drain_id=2,
        name=payload.name,
        address=payload.address,
        latitude=payload.latitude,
        longitude=payload.longitude,
        hardware_id=payload.hardware_id,
        is_active=payload.is_active,
    )

    repository.get_by_hardware_id.return_value = None
    repository.create.return_value = created_entity

    result = await service.create_drain(payload)

    assert isinstance(result, DrainRead)
    assert result.id == created_entity.id
    assert result.hardware_id == payload.hardware_id
    assert result.name == payload.name
    assert result.created_at == created_entity.created_at
    repository.get_by_hardware_id.assert_awaited_once_with(payload.hardware_id)
    repository.create.assert_awaited_once_with(payload)


@pytest.mark.asyncio
async def test_update_drain_success(repository: AsyncMock):
    service = DrainService(repository)
    current_entity = build_drain_entity(drain_id=10, hardware_id="HW-010")
    updated_entity = build_drain_entity(
        drain_id=10,
        name="Bueiro Atualizado",
        address="Rua Atualizada, 200",
        latitude=-23.2,
        longitude=-46.2,
        hardware_id="HW-010",
        is_active=False,
    )
    payload = DrainUpdate(
        name="Bueiro Atualizado",
        address="Rua Atualizada, 200",
        latitude=-23.2,
        longitude=-46.2,
        is_active=False,
    )

    repository.get_by_id.return_value = current_entity
    repository.update.return_value = updated_entity

    result = await service.update_drain(current_entity.id, payload)

    assert isinstance(result, DrainRead)
    assert result.id == current_entity.id
    assert result.name == payload.name
    assert result.is_active is False
    repository.get_by_id.assert_awaited_once_with(current_entity.id)
    repository.get_by_hardware_id.assert_not_awaited()
    repository.update.assert_awaited_once_with(current_entity, payload)


@pytest.mark.asyncio
async def test_get_drain_by_id_not_found_raises_404(repository: AsyncMock):
    service = DrainService(repository)
    repository.get_by_id.return_value = None

    with pytest.raises(HTTPException) as exc_info:
                await service.get_drain_by_id(999)

    assert exc_info.value.status_code == 404
    assert exc_info.value.detail == "Drain não encontrado"
    repository.get_by_id.assert_awaited_once_with(999)


@pytest.mark.asyncio
async def test_create_drain_duplicate_hardware_id_raises_400(repository: AsyncMock):
    service = DrainService(repository)
    payload = DrainCreate(
        name="Bueiro Sul",
        address="Rua Sul, 50",
        latitude=-23.3,
        longitude=-46.3,
        hardware_id="HW-999",
        is_active=True,
    )
    existing_entity = build_drain_entity(drain_id=99, hardware_id=payload.hardware_id)
    repository.get_by_hardware_id.return_value = existing_entity

    with pytest.raises(HTTPException) as exc_info:
        await service.create_drain(payload)

    assert exc_info.value.status_code == 400
    assert exc_info.value.detail == f"hardware_id '{payload.hardware_id}' já está em uso."
    repository.get_by_hardware_id.assert_awaited_once_with(payload.hardware_id)
    repository.create.assert_not_awaited()