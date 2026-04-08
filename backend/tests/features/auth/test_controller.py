import pytest
from httpx import AsyncClient
from unittest.mock import patch, MagicMock


@pytest.mark.asyncio
async def test_register_user(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mock_user = User(email="test@test.com", full_name="Test", role="User")
    mocker.patch("app.features.auth.service.AuthService.register_user", return_value=mock_user)
    
    response = await test_client.post(
        "/auth/register",
        json={"email": "test@test.com", "password": "password", "role": "USER"}
    )
    assert response.status_code == 201


@pytest.mark.asyncio
async def test_login_user(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mock_user = User(email="test@test.com", full_name="Test", role="User")
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
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData

    mock_user = UserTokenData(email="user@test.com", jti="123", role="User")
    app.dependency_overrides[get_current_user] = lambda: mock_user

    mocker.patch("app.features.auth.service.AuthService.logout", return_value=None)
    
    try:
        response = await test_client.post(
            "/auth/logout",
            headers={"Authorization": "Bearer fake_token"}
        )
        assert response.status_code == 200
    finally:
        app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_users_me_unauthorized(test_client: AsyncClient):
    # No auth header
    response = await test_client.get("/auth/users/me")
    assert response.status_code == 401


@pytest.mark.asyncio
async def test_users_me_authorized(test_client: AsyncClient, mocker):
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData, UserInDB
    
    mock_user = UserTokenData(email="user@test.com", jti="123", role="User")
    app.dependency_overrides[get_current_user] = lambda: mock_user

    # Controller requires DB user through repository
    mock_db_user = UserInDB(email="user@test.com", full_name="User", role="User", hashed_password="pw")
    mocker.patch("app.features.auth.repository.AuthRepository.get_user_by_email", return_value=mock_db_user)

    try:
        response = await test_client.get(
            "/auth/users/me",
            headers={"Authorization": "Bearer valid_token"}
        )
        assert response.status_code == 200
    finally:
        app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_rbac_user_access_blocked(test_client: AsyncClient, mocker):
    from app.main import app
    from app.extensions.auth import get_current_user
    from app.features.auth.dto import UserTokenData
    
    mock_user = UserTokenData(email="user@test.com", jti="123", role="User")
    app.dependency_overrides[get_current_user] = lambda: mock_user

    try:
        # For RoleChecker, we need to try an admin route, there is none in /auth so we will hit a generic one that uses it or we can just mock a test endpoint using it.
        # For simplicity, testing the /home/carousel admin endpoint
        response = await test_client.post(
            "/home/carousel",
            headers={"Authorization": "Bearer valid_token"},
            json={"title": "Test", "subtitle": "Desc", "image_url": "http://img.com", "action_url": "http://link.com", "order": 1, "section": "hero"}
        )
        # Assuming role checker blocks it
        assert response.status_code == 403
    finally:
        app.dependency_overrides.pop(get_current_user, None)


@pytest.mark.asyncio
async def test_rate_limit_login(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mock_user = User(email="test@test.com", full_name="Test", role="User")
    mocker.patch("app.features.auth.service.AuthService.authenticate_user", return_value=mock_user)
    mocker.patch("app.features.auth.service.AuthService.create_access_token", return_value="fake_token")
    
    from app.extensions.infrastructure import infrastructure
    from unittest.mock import AsyncMock
    
    mock_redis = MagicMock()
    mock_redis.get = AsyncMock(side_effect=[None, "1", "2", "3", "4", "5"])
    
    mock_pipe = MagicMock()
    mock_pipe.incr = AsyncMock()
    mock_pipe.expire = AsyncMock()
    mock_pipe.execute = AsyncMock()
    mock_redis.pipeline.return_value = mock_pipe
    
    original_redis = infrastructure.redis_client
    infrastructure.redis_client = mock_redis
    
    try:
        # Realiza 5 requisições simuladas que devem passar (status 200)
        for _ in range(5):
            response = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
            assert response.status_code == 200

        # A 6ª requisição deve ser bloqueada pelo RateLimiter (status 429)
        response_blocked = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
        assert response_blocked.status_code == 429
    finally:
        # Limpa o override
        infrastructure.redis_client = original_redis