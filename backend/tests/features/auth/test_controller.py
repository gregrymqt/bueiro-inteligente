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

    from app.main import app
    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123")

    mocker.patch("app.features.auth.service.AuthService.logout", return_value=None)
    
    response = await test_client.post(
        "/auth/logout",
        headers={"Authorization": "Bearer fake_token"}
    )
    assert response.status_code == 200

    app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_users_me_unauthorized(test_client: AsyncClient):
    # No auth header
    response = await test_client.get("/auth/users/me")
    assert response.status_code == 401


@pytest.mark.asyncio
async def test_users_me_authorized(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    from app.main import app
    from app.extensions.auth import get_current_user

    app.dependency_overrides[get_current_user] = lambda: UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123")

    # We must patch get_auth_service because it's a Depends dependency injected
    from unittest.mock import AsyncMock
    mock_auth_service = MagicMock()
    mock_auth_service.repository.get_user_by_email = AsyncMock()
    from app.features.auth.dto import UserInDB
    mock_auth_service.repository.get_user_by_email.return_value = UserInDB(email="test@test.com", full_name="Test", role="User", hashed_password="hashed")

    from app.features.auth.controller import get_auth_service
    app.dependency_overrides[get_auth_service] = lambda: mock_auth_service
    
    response = await test_client.get(
        "/auth/users/me",
        headers={"Authorization": "Bearer valid_token"}
    )
    assert response.status_code == 200

    app.dependency_overrides.pop(get_current_user, None)
    app.dependency_overrides.pop(get_auth_service, None)


@pytest.mark.asyncio
async def test_rbac_user_access_blocked(test_client: AsyncClient, mocker):
    from app.features.auth.dto import UserTokenData
    from app.main import app
    from app.extensions.auth import get_current_user

    # Mock user as User
    mock_user = UserTokenData(email="test@test.com", sub="test@test.com", role="User", jti="jti123")
    app.dependency_overrides[get_current_user] = lambda: mock_user

    # Simulating accessing an admin route (e.g., /auth/admin doesn't exist, we'll try /home/carousel post instead as it requires Admin)
    response = await test_client.post(
        "/home/carousel",
        headers={"Authorization": "Bearer valid_token"},
        json={"title": "Test", "description": "Desc", "image_url": "http://img.com", "link": "/link", "order": 1, "section": "hero"}
    )
    # Assuming role checker blocks it
    assert response.status_code == 403

    app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_rate_limit_login(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mocker.patch("app.features.auth.service.AuthService.authenticate_user", return_value=User(email="test@test.com", full_name="Test", role="User"))
    mocker.patch("app.features.auth.service.AuthService.create_access_token", return_value="fake_token")
    
    # Mock do Redis (cache) para simular o Rate Limit
    from unittest.mock import AsyncMock
    from app.extensions.infrastructure import InfrastructureExtension
    
    mock_redis = AsyncMock()
    # Simula a contagem de requisições (get the current count before hitting limit)
    mock_redis.get.side_effect = [None, b'1', b'2', b'3', b'4', b'5']

    mock_pipeline = AsyncMock()
    mock_redis.pipeline.return_value = mock_pipeline

    # The actual attribute used in app.core.security.RateLimiter is `infra.redis_client`
    infra_mock = InfrastructureExtension()
    infra_mock.redis_client = mock_redis
    
    # Realiza 5 requisições simuladas que devem passar (status 200)
    for _ in range(5):
        response = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
        assert response.status_code == 200
    
    # A 6ª requisição deve ser bloqueada pelo RateLimiter (status 429)
    response_blocked = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
    assert response_blocked.status_code == 429