import pytest
from httpx import AsyncClient
from unittest.mock import MagicMock, AsyncMock


@pytest.mark.asyncio
async def test_get_home_public(test_client: AsyncClient, mocker):
    mocker.patch("app.features.home.service.HomeService.get_home_content", return_value={"carousels": [], "stats": []})
    
    response = await test_client.get("/home")
    
    assert response.status_code == 200
    assert "carousels" in response.json()
    assert "stats" in response.json()


@pytest.mark.asyncio
async def test_get_home_rate_limit(test_client: AsyncClient, mocker):
    mocker.patch("app.features.home.service.HomeService.get_home_content", return_value={"carousels": [], "stats": []})
    
    from app.extensions.infrastructure import infrastructure
    
    # Mock Redis client completely
    mock_redis = MagicMock()
    # Simulating values for `get(key)`: 10 allowed requests (1 to 10), then 11+
    mock_redis.get = AsyncMock(side_effect=[None, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"])
    
    # Mock pipeline operations
    mock_pipe = MagicMock()
    mock_pipe.incr = AsyncMock()
    mock_pipe.expire = AsyncMock()
    mock_pipe.execute = AsyncMock()
    mock_redis.pipeline.return_value = mock_pipe

    # Patch the singleton instance directly
    original_redis = infrastructure.redis_client
    infrastructure.redis_client = mock_redis
    
    try:
        for _ in range(10):
            response = await test_client.get("/home")
            assert response.status_code == 200

        blocked_response = await test_client.get("/home")
        assert blocked_response.status_code == 429
    finally:
        # Restore the original redis client
        infrastructure.redis_client = original_redis


@pytest.mark.asyncio
async def test_create_carousel_unauthorized_roles(test_client: AsyncClient, mocker):
    # Simulate a User
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData
    
    mock_user = UserTokenData(email="user@test.com", jti="123", role="User")
    app.dependency_overrides[get_current_user] = lambda: mock_user
    
    try:
        response = await test_client.post(
            "/home/carousel",
            headers={"Authorization": "Bearer fake_token"},
            json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link"}
        )

        assert response.status_code == 403
    finally:
        app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_create_carousel_admin_success(test_client: AsyncClient, mocker):
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData

    mock_admin = UserTokenData(email="admin@test.com", jti="123", role="Admin")
    app.dependency_overrides[get_current_user] = lambda: mock_admin
    
    from app.features.home.dto import CarouselDTO
    import uuid
    mock_return = CarouselDTO(
        title="Test",
        subtitle="Desc",
        image_url="http://img.com/",
        action_url="http://link.com/",
        order=1,
        section="hero",
        id=uuid.uuid4()
    )
    mocker.patch("app.features.home.service.HomeService.create_carousel_item", return_value=mock_return)
    
    try:
        response = await test_client.post(
            "/home/carousel",
            headers={"Authorization": "Bearer admin_token"},
            json={"title": "Test", "subtitle": "Desc", "image_url": "http://img.com", "action_url": "http://link.com", "order": 1, "section": "hero"}
        )

        assert response.status_code in [200, 201]
    finally:
        app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_update_stat_not_found(test_client: AsyncClient, mocker):
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData

    mock_admin = UserTokenData(email="admin@test.com", jti="123", role="Admin")
    app.dependency_overrides[get_current_user] = lambda: mock_admin
    
    from fastapi import HTTPException
    mocker.patch("app.features.home.service.HomeService.update_stat_card", side_effect=HTTPException(status_code=404, detail="Stat not found"))
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    try:
        response = await test_client.patch(
            f"/home/stats/{fake_uuid}",
            headers={"Authorization": "Bearer admin_token"},
            json={"value": "1000"}
        )

        assert response.status_code == 404
        assert response.json()["detail"] == "Stat not found"
    finally:
        app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_delete_carousel_admin_success(test_client: AsyncClient, mocker):
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData
    
    mock_admin = UserTokenData(email="admin@test.com", jti="123", role="Admin")
    app.dependency_overrides[get_current_user] = lambda: mock_admin
    mocker.patch("app.features.home.service.HomeService.delete_carousel_item", return_value=True)
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    try:
        response = await test_client.delete(
            f"/home/carousel/{fake_uuid}",
            headers={"Authorization": "Bearer admin_token"}
        )

        assert response.status_code in [200, 204]
    finally:
        app.dependency_overrides.pop(get_current_user, None)
