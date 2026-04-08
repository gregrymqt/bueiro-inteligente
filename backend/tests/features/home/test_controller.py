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
    
    mock_redis = AsyncMock()
    # 10 reqs passing, 11th blocking
    mock_redis.get.side_effect = [None] + [str(i) for i in range(1, 11)]
    mocker.patch.object(infrastructure, "redis_client", mock_redis)
    
    for _ in range(10):
        response = await test_client.get("/home")
        assert response.status_code == 200
        
    blocked_response = await test_client.get("/home")
    assert blocked_response.status_code == 429


@pytest.mark.asyncio
async def test_create_carousel_unauthorized_roles(test_client: AsyncClient, mocker):
    # Simulate a User
    from app.features.auth.dto import UserTokenData
    mock_user = UserTokenData(email="user@test.com", role="User", jti="valid")

    from app.main import app
    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: mock_user
    
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer fake_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link"}
    )
    
    assert response.status_code == 403


@pytest.mark.asyncio
async def test_create_carousel_admin_success(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    mock_admin = UserTokenData(email="admin@test.com", role="Admin", jti="valid")

    from app.main import app
    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: mock_admin

    from app.features.home.dto import CarouselDTO
    from uuid import uuid4
    mock_dto = CarouselDTO(id=uuid4(), title="Test", subtitle="Desc", image_url="http://img.com", action_url="http://link.com", section="hero", order=1)
    mocker.patch("app.features.home.service.HomeService.create_carousel_item", return_value=mock_dto)
    
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer admin_token"},
        json={"title": "Test", "subtitle": "Desc", "image_url": "http://img.com", "action_url": "http://link.com", "order": 1, "section": "hero"}
    )
    
    assert response.status_code in [200, 201]


@pytest.mark.asyncio
async def test_update_stat_not_found(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    mock_admin = UserTokenData(email="admin@test.com", role="Admin", jti="valid")

    from app.main import app
    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: mock_admin
    
    from fastapi import HTTPException
    mocker.patch("app.features.home.service.HomeService.update_stat_card", side_effect=HTTPException(status_code=404, detail="Stat not found"))
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    response = await test_client.patch(
        f"/home/stats/{fake_uuid}",
        headers={"Authorization": "Bearer admin_token"},
        json={"value": "1000"}
    )
    
    assert response.status_code == 404
    assert response.json()["detail"] == "Stat not found"


@pytest.mark.asyncio
async def test_delete_carousel_admin_success(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    mock_admin = UserTokenData(email="admin@test.com", role="Admin", jti="valid")

    from app.main import app
    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: mock_admin
    mocker.patch("app.features.home.service.HomeService.delete_carousel_item", return_value=True)
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    response = await test_client.delete(
        f"/home/carousel/{fake_uuid}",
        headers={"Authorization": "Bearer admin_token"}
    )
    
    assert response.status_code in [200, 204]
