import pytest
from httpx import AsyncClient
from unittest.mock import patch, MagicMock


@pytest.mark.asyncio
async def test_register_user(test_client: AsyncClient, mocker):
    from app.features.auth.dto import User
    mocker.patch("app.features.auth.service.AuthService.register_user", return_value=User(email="test@test.com", full_name="Test User", role="USER"))
    
    response = await test_client.post(
        "/auth/register",
        json={"email": "test@test.com", "password": "password", "role": "USER"}
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
    app.dependency_overrides[get_current_user] = lambda: UserTokenData(email="test@test.com", jti="valid_jti", role="User")
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
    from app.features.auth.models import User as DBUser, Role as DBRole

    from app.main import app
    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: UserTokenData(email="test@test.com", jti="valid_jti", role="User")

    # Mocking get_user_by_email since the route hits the DB to get the actual user.
    mock_db_user = MagicMock()
    mock_db_user.email = "test@test.com"
    mock_db_user.full_name = "Test User"
    mock_db_user.roles = ["User"]
    mocker.patch("app.features.auth.repository.AuthRepository.get_user_by_email", return_value=mock_db_user)
    
    response = await test_client.get(
        "/auth/users/me",
        headers={"Authorization": "Bearer valid_token"}
    )
    assert response.status_code == 200


@pytest.mark.asyncio
async def test_rbac_user_access_blocked(test_client: AsyncClient, mocker):
    from app.main import app
    from fastapi import APIRouter, Depends
    from app.extensions.auth import RoleChecker

    # Adicionando uma rota temporária para o teste
    test_router = APIRouter()
    @test_router.get("/test_admin_route", dependencies=[Depends(RoleChecker(['Admin']))])
    async def temp_admin_route():
        return {"msg": "ok"}
    app.include_router(test_router)

    # Mock user as Role.USER
    from app.features.auth.dto import UserTokenData
    mock_user = UserTokenData(email="test@test.com", jti="valid_jti", role="User")

    from app.extensions.auth import get_current_user
    app.dependency_overrides[get_current_user] = lambda: mock_user
    
    response = await test_client.get(
        "/test_admin_route",
        headers={"Authorization": "Bearer valid_token"}
    )

    assert response.status_code == 403


@pytest.mark.asyncio
async def test_rate_limit_login(test_client: AsyncClient, mocker):
    mocker.patch("app.features.auth.service.AuthService.authenticate_user", return_value=MagicMock())
    mocker.patch("app.features.auth.service.AuthService.create_access_token", return_value="fake_token")
    
    # Mock do Redis (cache) para simular o Rate Limit
    from app.extensions.infrastructure import InfrastructureExtension
    from unittest.mock import AsyncMock
    
    mock_redis = AsyncMock()
    # Simula a contagem de requisições: 1 a 5 passam, a 6ª excede o limite
    mock_redis.get.side_effect = [None, "1", "2", "3", "4", "5"]

    # Mocking InfrastructureExtension's redis_client
    from app.extensions.infrastructure import infrastructure
    mocker.patch.object(infrastructure, "redis_client", mock_redis)
    
    # Realiza 5 requisições simuladas que devem passar (status 200)
    for _ in range(5):
        response = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
        assert response.status_code == 200
    
    # A 6ª requisição deve ser bloqueada pelo RateLimiter (status 429)
    response_blocked = await test_client.post("/auth/login", json={"email": "test@test.com", "password": "pass"})
    assert response_blocked.status_code == 429
