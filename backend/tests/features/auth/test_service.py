import pytest
from unittest.mock import AsyncMock, patch, ANY
from fastapi import HTTPException
from app.features.auth.service import AuthService
from app.features.auth.dto import UserCreate, UserLogin
from app.features.auth.models import User, Role


@pytest.fixture
def auth_service():
    repository = AsyncMock()
    cache = AsyncMock()
    return AuthService(repository, cache)


@pytest.mark.asyncio
async def test_register_user_success(auth_service):
    # Setup
    auth_service.repository.get_user_by_email.return_value = None
    auth_service.repository.create_user.return_value = User(id=1, email="test@test.com", role=Role.USER)
    user_dto = UserCreate(email="test@test.com", password="password123")

    # Execute
    user = await auth_service.register_user(user_dto)

    # Assert
    assert user.email == "test@test.com"
    auth_service.repository.get_user_by_email.assert_called_once_with("test@test.com")
    auth_service.repository.create_user.assert_called_once()
    

@pytest.mark.asyncio
async def test_register_user_fail_email_exists(auth_service):
    # Setup
    auth_service.repository.get_user_by_email.return_value = User(id=1, email="exist@test.com", role=Role.USER)
    user_dto = UserCreate(email="exist@test.com", password="password123")

    # Execute and Assert
    with pytest.raises(HTTPException) as exc_info:
        await auth_service.register_user(user_dto)
    assert exc_info.value.status_code == 400
    assert "Email already registered" in exc_info.value.detail


@pytest.mark.asyncio
@patch("app.extensions.auth.verify_password")
async def test_authenticate_user_success(mock_verify_password, auth_service):
    # Setup
    user = User(id=1, email="test@test.com", hashed_password="hashed_password", role=Role.USER)
    auth_service.repository.get_user_by_email.return_value = user
    user_dto = UserLogin(email="test@test.com", password="password123")
    mock_verify_password.return_value = True

    # Execute
    result_user = await auth_service.authenticate_user(user_dto)

    # Assert
    assert result_user is not None
    assert result_user.email == "test@test.com"


@pytest.mark.asyncio
@patch("app.extensions.auth.verify_password")
async def test_authenticate_user_fail(mock_verify_password, auth_service):
    # Setup
    auth_service.repository.get_user_by_email.return_value = None
    user_dto = UserLogin(email="notfound@test.com", password="password123")

    # Execute and Assert
    with pytest.raises(HTTPException) as exc_info:
        await auth_service.authenticate_user(user_dto)
    assert exc_info.value.status_code == 401


@pytest.mark.asyncio
@patch("app.extensions.auth.create_access_token")
async def test_create_access_token_call(mock_create_access_token, auth_service):
    # Setup
    user = User(id=1, email="test@test.com", role=Role.USER)
    mock_create_access_token.return_value = "token123"

    # Execute
    token = auth_service.create_access_token(user)

    # Assert
    assert token == "token123"
    mock_create_access_token.assert_called_once()


@pytest.mark.asyncio
@patch("app.extensions.auth.decode_access_token")
async def test_logout(mock_decode, auth_service):
    # Setup
    mock_decode.return_value = {"jti": "jti123"}
    auth_service.cache.set.return_value = None

    # Execute
    await auth_service.logout("valid_token")

    # Assert
    mock_decode.assert_called_once_with("valid_token")
    
    # Valida apenas se foi chamado com a chave começando com a blacklist
    args, kwargs = auth_service.cache.set.call_args
    assert args[0].startswith("blacklist:")
    assert args[1] == "true"
