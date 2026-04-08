import pytest
from unittest.mock import AsyncMock, patch, ANY
from fastapi import HTTPException
from app.features.auth.service import AuthService
from app.features.auth.dto import UserCreate, LoginRequest
from app.features.auth.models import User, Role


@pytest.fixture
def auth_service():
    repository = AsyncMock()
    return AuthService(repository)


@pytest.mark.asyncio
@patch("app.extensions.auth.AuthExtension.get_password_hash", new_callable=AsyncMock)
async def test_register_user_success(mock_get_password_hash, auth_service):
    # Setup
    from app.features.auth.dto import User as UserDTO
    auth_service.repository.get_user_by_email.return_value = None
    auth_service.repository.create_user.return_value = UserDTO(email="test@test.com", full_name="Test User", role="User")
    mock_get_password_hash.return_value = "hashed"
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
    from app.features.auth.dto import User as UserDTO
    auth_service.repository.get_user_by_email.return_value = UserDTO(email="exist@test.com", full_name="Exists", role="User")
    user_dto = UserCreate(email="exist@test.com", password="password123")

    # Execute and Assert
    with pytest.raises(HTTPException) as exc_info:
        await auth_service.register_user(user_dto)
    assert exc_info.value.status_code == 400
    assert "Email already registered" in exc_info.value.detail


@pytest.mark.asyncio
@patch("app.extensions.auth.AuthExtension.verify_password", new_callable=AsyncMock)
async def test_authenticate_user_success(mock_verify_password, auth_service):
    # Setup
    from app.features.auth.dto import UserInDB
    user = UserInDB(email="test@test.com", full_name="Test", role="User", hashed_password="hashed_password")
    auth_service.repository.get_user_by_email.return_value = user
    mock_verify_password.return_value = True

    # Execute (authenticate_user expects email and password as separate args)
    result_user = await auth_service.authenticate_user("test@test.com", "password123")

    # Assert
    assert result_user is not None
    assert result_user.email == "test@test.com"


@pytest.mark.asyncio
@patch("app.extensions.auth.AuthExtension.verify_password", new_callable=AsyncMock)
async def test_authenticate_user_fail(mock_verify_password, auth_service):
    # Setup
    auth_service.repository.get_user_by_email.return_value = None

    # Execute and Assert
    result = await auth_service.authenticate_user("notfound@test.com", "password123")
    assert result is None


@pytest.mark.asyncio
@patch("app.extensions.auth.AuthExtension.create_access_token")
async def test_create_access_token_call(mock_create_access_token, auth_service):
    # Setup
    from app.features.auth.dto import User as UserDTO
    user = UserDTO(email="test@test.com", full_name="Test", role="User")
    mock_create_access_token.return_value = "token123"

    # Execute
    token = auth_service.create_access_token(user)

    # Assert
    assert token == "token123"
    mock_create_access_token.assert_called_once()


@pytest.mark.asyncio
@patch("app.extensions.auth.AuthExtension.add_to_blacklist", new_callable=AsyncMock)
async def test_logout(mock_blacklist, auth_service):
    # Setup
    jti = "jti123"

    # Execute
    await auth_service.logout(jti)

    # Assert
    mock_blacklist.assert_called_once_with(jti)
