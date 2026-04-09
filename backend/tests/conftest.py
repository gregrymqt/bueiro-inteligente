import os
import pytest
import pytest_asyncio
from unittest.mock import MagicMock, AsyncMock

# Set environment variables for tests before modules are imported
os.environ["DATABASE_URL"] = "postgresql+asyncpg://mock:mock@localhost/mockdb"
os.environ["DATABASE_URL_LOCAL"] = "postgresql+asyncpg://mock:mock@localhost/mockdb"

from httpx import AsyncClient, ASGITransport

@pytest.fixture(scope="session")
def anyio_backend():
    return "asyncio"

# Se o pytest_asyncio for usado com fixtures assíncronas
@pytest.fixture(scope="session")
def event_loop_policy():
    import asyncio
    return asyncio.DefaultEventLoopPolicy()

@pytest_asyncio.fixture
async def test_client():
    from app.main import app
    from app.extensions.infrastructure import get_db, get_cache, infrastructure

    # Mock the infrastructure singleton
    infrastructure.redis_client = AsyncMock()
    infrastructure.session_factory = MagicMock()

    # Overrides for FastAPI dependency injection
    async def mock_get_db():
        yield AsyncMock()

    async def mock_get_cache():
        return AsyncMock()

    app.dependency_overrides[get_db] = mock_get_db
    app.dependency_overrides[get_cache] = mock_get_cache

    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as client:
        yield client

    app.dependency_overrides.clear()


@pytest_asyncio.fixture
async def db_session():
    from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine
    from sqlalchemy.pool import StaticPool

    from app.core.database import Base
    from app.features.monitoring import models as monitoring_models  # noqa: F401

    engine = create_async_engine(
        "sqlite+aiosqlite:///:memory:",
        echo=False,
        connect_args={"check_same_thread": False},
        poolclass=StaticPool,
    )

    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)

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
        async with engine.begin() as conn:
            await conn.run_sync(Base.metadata.drop_all)
        await engine.dispose()
