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
