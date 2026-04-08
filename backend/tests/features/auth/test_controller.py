import pytest
from httpx import AsyncClient
from unittest.mock import patch, MagicMock


@pytest.mark.asyncio
async def test_register_user(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mocker.patch("app.features.auth.service.AuthService.register_user", return_value=User(email="test@test.com", full_name="Test", role="User"))
    
    response = await test_client.post(
        "/auth/register",
        json={"email": "test@test.com", "password": "password", "role": "User", "full_name": "Test"}
    )
    assert response.status_code == 201


@pytest.mark.asyncio
async def test_login_user(test_client: AsyncClient, mocker):
    mock_user = MagicMock(id=1, email="test@test.com")
    mocker.patch("app.features.auth.service.AuthService.authenticate_user", return_value=mock_user)
    mocker.patch("app.features.auth.service.AuthService.create_access_token", return_value="fake_token")
    
    response = await test_client.post(
        "/auth/login",
        json={"email": "test@test.com", "password": "password"}
    )
    assert response.status_code == 200
    assert "access_token" in response.json()


@pytest.mark.asyncio
async def test_logout_user(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    mocker.patch("app.extensions.auth.get_current_user", return_value=UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123"))
    mocker.patch("app.features.auth.service.AuthService.logout", return_value=None)
    
    response = await test_client.post(
        "/auth/logout",
        headers={"Authorization": "Bearer fake_token"}
    )
    assert response.status_code == 200


@pytest.mark.asyncio
async def test_users_me_unauthorized(test_client: AsyncClient):
    # No auth header
    response = await test_client.get("/auth/users/me")
    assert response.status_code == 401


@pytest.mark.asyncio
async def test_users_me_authorized(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    mocker.patch("app.extensions.auth.get_current_user", return_value=UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123"))
    
    response = await test_client.get(
        "/auth/users/me",
        headers={"Authorization": "Bearer valid_token"}
    )
    assert response.status_code == 200


@pytest.mark.asyncio
async def test_rbac_user_access_blocked(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    # Mock user as User
    mock_user = UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123")
    mocker.patch("app.extensions.auth.get_current_user", return_value=mock_user)
    
    # Simulating accessing an admin route (e.g., /auth/admin doesn't exist, we'll try /home/carousel post instead as it requires Admin)
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer valid_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link", "order": 1, "section": "hero"}
    )
    # Assuming role checker blocks it
    assert response.status_code == 403


@pytest.mark.asyncio
async def test_rate_limit_login(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mocker.patch("app.features.auth.service.AuthService.authenticate_user", return_value=User(email="test@test.com", full_name="Test", role="User"))
    mocker.patch("app.features.auth.service.AuthService.create_access_token", return_value="fake_token")
    
    # Mock do Redis (cache) para simular o Rate Limit
    from app.extensions.infrastructure import get_cache
    from app.main import app
    from unittest.mock import AsyncMock
    
    mock_redis = AsyncMock()
    # Simula a contagem de requisições: 1 a 5 passam, a 6ª excede o limite
    mock_pipeline = AsyncMock()
    mock_pipeline.execute.side_effect = [[1], [2], [3], [4], [5], [6]]
    mock_redis.pipeline.return_value = mock_pipeline
    app.dependency_overrides[get_cache] = lambda: mock_redis
    
    # Realiza 5 requisições simuladas que devem passar (status 200)
    for _ in range(5):
        response = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
        assert response.status_code == 200
    
    # A 6ª requisição deve ser bloqueada pelo RateLimiter (status 429)
    response_blocked = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
    assert response_blocked.status_code == 429
    
    # Limpa o override
    app.dependency_overrides.pop(get_cache, None)