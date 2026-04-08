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
@patch("app.extensions.auth.auth_extension.get_password_hash")
async def test_register_user_success(mock_get_password_hash, auth_service):
    # Setup
    from app.features.auth.dto import UserInDB
    auth_service.repository.get_user_by_email.return_value = None
    auth_service.repository.create_user.return_value = UserInDB(email="test@test.com", full_name="Test", role="User", hashed_password="hashed_password")
    user_dto = UserCreate(email="test@test.com", password="password123", full_name="Test", role="User")
    mock_get_password_hash.return_value = "hashed_password"

    # Execute
    user = await auth_service.register_user(user_dto)

    # Assert
    assert user.email == "test@test.com"
    auth_service.repository.get_user_by_email.assert_called_once_with("test@test.com")
    auth_service.repository.create_user.assert_called_once()
    

@pytest.mark.asyncio
async def test_register_user_fail_email_exists(auth_service):
    # Setup
    from app.features.auth.dto import UserInDB
    auth_service.repository.get_user_by_email.return_value = UserInDB(email="exist@test.com", full_name="Exist", role="User", hashed_password="hashed")
    user_dto = UserCreate(email="exist@test.com", password="password123", full_name="Exist", role="User")

    # Execute and Assert
    with pytest.raises(HTTPException) as exc_info:
        await auth_service.register_user(user_dto)
    assert exc_info.value.status_code == 400
    assert "Email already registered" in exc_info.value.detail


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.verify_password")
async def test_authenticate_user_success(mock_verify_password, auth_service):
    # Setup
    from app.features.auth.dto import UserInDB
    user = UserInDB(email="test@test.com", hashed_password="hashed_password", role="User", full_name="Test")
    auth_service.repository.get_user_by_email.return_value = user
    user_dto = LoginRequest(email="test@test.com", password="password123")
    mock_verify_password.return_value = True

    # Execute
    result_user = await auth_service.authenticate_user(user_dto.email, user_dto.password)

    # Assert
    assert result_user is not None
    assert result_user.email == "test@test.com"


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.verify_password")
async def test_authenticate_user_fail(mock_verify_password, auth_service):
    # Setup
    auth_service.repository.get_user_by_email.return_value = None
    user_dto = LoginRequest(email="notfound@test.com", password="password123")

    # Execute
    result_user = await auth_service.authenticate_user(user_dto.email, user_dto.password)

    # Assert
    assert result_user is None


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.create_access_token")
async def test_create_access_token_call(mock_create_access_token, auth_service):
    # Setup
    from app.features.auth.dto import User as DTOUser
    user = DTOUser(email="test@test.com", role="User", full_name="Test")
    mock_create_access_token.return_value = "token123"

    # Execute
    token = auth_service.create_access_token(user)

    # Assert
    assert token == "token123"
    mock_create_access_token.assert_called_once()


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.add_to_blacklist")
async def test_logout(mock_add_to_blacklist, auth_service):
    # Execute
    await auth_service.logout("valid_jti")

    # Assert
    mock_add_to_blacklist.assert_called_once_with("valid_jti")
