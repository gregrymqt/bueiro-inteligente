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
    
    from unittest.mock import AsyncMock
    from app.extensions.infrastructure import InfrastructureExtension
    
    mock_redis = AsyncMock()
    # 10 reqs passing, 11th blocking
    mock_redis.get.side_effect = [None, b'1', b'2', b'3', b'4', b'5', b'6', b'7', b'8', b'9', b'10']

    mock_pipeline = AsyncMock()
    mock_redis.pipeline.return_value = mock_pipeline

    infra_mock = InfrastructureExtension()
    infra_mock.redis_client = mock_redis
    
    for _ in range(10):
        response = await test_client.get("/home")
        assert response.status_code == 200
        
    blocked_response = await test_client.get("/home")
    assert blocked_response.status_code == 429


@pytest.mark.asyncio
async def test_create_carousel_unauthorized_roles(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    from app.main import app
    from app.extensions.auth import get_current_user

    # Simulate a User
    mock_user = UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123")
    app.dependency_overrides[get_current_user] = lambda: mock_user
    
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer fake_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link", "order": 1, "section": "hero"}
    )
    
    assert response.status_code == 403
    app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_create_carousel_admin_success(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    from app.main import app
    from app.extensions.auth import get_current_user
    from unittest.mock import AsyncMock

    mock_admin = UserTokenData(email="admin@test.com", sub="admin@test.com", role="Admin", jti="jti123")
    app.dependency_overrides[get_current_user] = lambda: mock_admin

    # We must patch get_home_service because it's a Depends dependency injected
    mock_home_service = MagicMock()
    import uuid
    from app.features.home.dto import CarouselDTO
    mock_home_service.create_carousel_item = AsyncMock(return_value=CarouselDTO(id=uuid.uuid4(), title="Test", image_url="http://img.com", order=1, section="hero"))

    from app.features.home.controller import get_home_service
    app.dependency_overrides[get_home_service] = lambda: mock_home_service
    
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer admin_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link", "order": 1, "section": "hero"}
    )
    
    assert response.status_code in [200, 201]
    app.dependency_overrides.pop(get_current_user, None)
    app.dependency_overrides.pop(get_home_service, None)


@pytest.mark.asyncio
async def test_update_stat_card_item_not_found(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    from app.main import app
    from app.extensions.auth import get_current_user

    mock_admin = UserTokenData(email="admin@test.com", sub="admin@test.com", role="Admin", jti="jti123")
    app.dependency_overrides[get_current_user] = lambda: mock_admin

    from unittest.mock import AsyncMock
    mock_home_service = MagicMock()
    mock_home_service.update_stat_card = AsyncMock(return_value=None)
    
    from app.features.home.controller import get_home_service
    app.dependency_overrides[get_home_service] = lambda: mock_home_service
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    response = await test_client.patch(
        f"/home/stats/{fake_uuid}",
        headers={"Authorization": "Bearer admin_token"},
        json={"value": "1000"}
    )
    
    assert response.status_code == 404
    assert response.json()["detail"] == "Stat card não encontrado."
    app.dependency_overrides.pop(get_current_user, None)
    app.dependency_overrides.pop(get_home_service, None)


@pytest.mark.asyncio
async def test_delete_carousel_admin_success(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    from app.main import app
    from app.extensions.auth import get_current_user

    mock_admin = UserTokenData(email="admin@test.com", sub="admin@test.com", role="Admin", jti="jti123")
    app.dependency_overrides[get_current_user] = lambda: mock_admin

    from unittest.mock import AsyncMock
    mock_home_service = MagicMock()
    mock_home_service.delete_carousel_item = AsyncMock(return_value=True)

    from app.features.home.controller import get_home_service
    app.dependency_overrides[get_home_service] = lambda: mock_home_service
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    response = await test_client.delete(
        f"/home/carousel/{fake_uuid}",
        headers={"Authorization": "Bearer admin_token"}
    )
    
    assert response.status_code in [200, 204]
    app.dependency_overrides.pop(get_current_user, None)
    app.dependency_overrides.pop(get_home_service, None)
