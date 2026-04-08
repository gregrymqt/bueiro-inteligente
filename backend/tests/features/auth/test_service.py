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
async def test_register_user_success(mock_hash, auth_service):
    # Setup
    mock_hash.return_value = "hashed_password"
    auth_service.repository.get_user_by_email.return_value = None

    class MockUser:
        email = "test@test.com"
        full_name = "Test User"
        hashed_password = "hashed_password"
        role = "User"

    auth_service.repository.create_user.return_value = MockUser()
    user_dto = UserCreate(email="test@test.com", password="password123")

    # Execute
    user = await auth_service.register_user(user_dto)

    # Assert
    assert user.email == "test@test.com"
    auth_service.repository.get_user_by_email.assert_called_once_with("test@test.com")
    auth_service.repository.create_user.assert_called_once()
    

@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.get_password_hash")
async def test_register_user_fail_email_exists(mock_hash, auth_service):
    # Setup
    mock_hash.return_value = "hashed_password"
    mock_role = Role(id=1, name="User")
    auth_service.repository.get_user_by_email.return_value = User(id=1, email="exist@test.com", role=mock_role)
    user_dto = UserCreate(email="exist@test.com", password="password123")

    # Execute and Assert
    with pytest.raises(HTTPException) as exc_info:
        await auth_service.register_user(user_dto)
    assert exc_info.value.status_code == 400
    assert "Email already registered" in exc_info.value.detail


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.verify_password")
async def test_authenticate_user_success(mock_verify_password, auth_service):
    # Setup
    mock_role = Role(id=1, name="User")
    # Simulate DB user object but map role to string if the service expects a string
    class MockUser:
        email = "test@test.com"
        full_name = "Test User"
        hashed_password = "hashed_password"
        role = "User"

    auth_service.repository.get_user_by_email.return_value = MockUser()
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

    # Execute and Assert
    result_user = await auth_service.authenticate_user(user_dto.email, user_dto.password)
    assert result_user is None


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.create_access_token")
async def test_create_access_token_call(mock_create_access_token, auth_service):
    # Setup
    from app.features.auth.dto import User as DtoUser
    user = DtoUser(email="test@test.com", role="User")
    mock_create_access_token.return_value = "token123"

    # Execute
    token = auth_service.create_access_token(user)

    # Assert
    assert token == "token123"
    mock_create_access_token.assert_called_once()


@pytest.mark.asyncio
@patch("app.extensions.auth.auth_extension.add_to_blacklist")
async def test_logout(mock_add_to_blacklist, auth_service):
    # Setup

    # Execute
    await auth_service.logout("valid_token_jti")

    # Assert
    mock_add_to_blacklist.assert_called_once_with("valid_token_jti")
