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
    
    from app.extensions.infrastructure import get_cache
    from app.main import app
    
    mock_redis = AsyncMock()
    # 10 reqs passing, 11th blocking
    mock_redis.incr.side_effect = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
    app.dependency_overrides[get_cache] = lambda: mock_redis
    
    for _ in range(10):
        response = await test_client.get("/home")
        assert response.status_code == 200
        
    blocked_response = await test_client.get("/home")
    assert blocked_response.status_code == 429
    
    app.dependency_overrides.pop(get_cache, None)


@pytest.mark.asyncio
async def test_create_carousel_unauthorized_roles(test_client: AsyncClient, mocker):
    # Simulate a User
    mock_user = MagicMock(role="USER")
    mocker.patch("app.extensions.auth.get_current_user", return_value=mock_user)
    
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer fake_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link"}
    )
    
    assert response.status_code == 403


@pytest.mark.asyncio
async def test_create_carousel_admin_success(test_client: AsyncClient, mocker):
    mock_admin = MagicMock(role="ADMIN")
    mocker.patch("app.extensions.auth.get_current_user", return_value=mock_admin)
    mocker.patch("app.features.home.service.HomeService.create_carousel_item", return_value=MagicMock(title="Test"))
    
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer admin_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link"}
    )
    
    assert response.status_code in [200, 201]


@pytest.mark.asyncio
async def test_update_stat_card_item_not_found(test_client: AsyncClient, mocker):
    mock_admin = MagicMock(role="ADMIN")
    mocker.patch("app.extensions.auth.get_current_user", return_value=mock_admin)
    
    mocker.patch("app.features.home.service.HomeService.update_stat_card", return_value=None)
    
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
    mock_admin = MagicMock(role="ADMIN")
    mocker.patch("app.extensions.auth.get_current_user", return_value=mock_admin)
    mocker.patch("app.features.home.service.HomeService.delete_carousel_item", return_value=None)
    
    fake_uuid = "123e4567-e89b-12d3-a456-426614174000"
    response = await test_client.delete(
        f"/home/carousel/{fake_uuid}",
        headers={"Authorization": "Bearer admin_token"}
    )
    
    assert response.status_code in [200, 204]
