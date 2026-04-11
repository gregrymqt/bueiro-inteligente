import os

os.environ.setdefault("DB_LOCAL", "true")
os.environ.setdefault("DATABASE_URL_LOCAL", "sqlite+aiosqlite:///:memory:")
os.environ.setdefault("DATABASE_URL_CLOUD", "sqlite+aiosqlite:///:memory:")

import pytest
import pytest_asyncio
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine
from sqlalchemy.pool import StaticPool

from app.core.database import Base
from app.features.drain.dto import DrainCreate
from app.features.drain.models import Drain
from app.features.drain.repository import DrainRepository


@pytest_asyncio.fixture
async def db_session() -> AsyncSession:
    engine = create_async_engine(
      "sqlite+aiosqlite:///:memory:",
      connect_args={"check_same_thread": False},
      poolclass=StaticPool,
      echo=False,
    )

    async with engine.begin() as connection:
        await connection.run_sync(Base.metadata.create_all)

    session_factory = async_sessionmaker(
        bind=engine,
        class_=AsyncSession,
        expire_on_commit=False,
    )
    session = session_factory()

    try:
        yield session
    finally:
        await session.close()
        async with engine.begin() as connection:
            await connection.run_sync(Base.metadata.drop_all)
        await engine.dispose()


@pytest.mark.asyncio
async def test_create_persists_and_searches_by_hardware_id(db_session: AsyncSession):
    repository = DrainRepository(db_session)
    payload = DrainCreate(
        name="Bueiro Industrial",
        address="Rua das Fábricas, 300",
        latitude=-23.501,
        longitude=-46.701,
        hardware_id="HW-101",
        is_active=True,
    )

    created = await repository.create(payload)
    fetched = await repository.get_by_hardware_id(payload.hardware_id)

    assert created.id is not None
    assert created.created_at is not None
    assert created.hardware_id == payload.hardware_id
    assert fetched is not None
    assert fetched.id == created.id
    assert fetched.hardware_id == payload.hardware_id
    assert fetched.name == payload.name


@pytest.mark.asyncio
async def test_delete_removes_drain(db_session: AsyncSession):
    repository = DrainRepository(db_session)
    payload = DrainCreate(
        name="Bueiro Residencial",
        address="Rua das Flores, 42",
        latitude=-23.502,
        longitude=-46.702,
        hardware_id="HW-202",
        is_active=False,
    )

    created = await repository.create(payload)

    assert await repository.get_by_id(created.id) is not None

    await repository.delete(created)

    assert await repository.get_by_hardware_id(payload.hardware_id) is None
    assert await repository.get_by_id(created.id) is None