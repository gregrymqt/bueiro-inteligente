import pytest
from unittest.mock import AsyncMock, MagicMock
from app.features.auth.repository import AuthRepository
from app.features.auth.models import User, Role
from sqlalchemy.ext.asyncio import AsyncSession


@pytest.fixture
def mock_session():
    session = AsyncMock(spec=AsyncSession)
    return session


@pytest.mark.asyncio
async def test_get_user_by_email_exists(mock_session):
    # Setup mock
    repository = AuthRepository(mock_session)
    mock_result = MagicMock()
    mock_user = User(id=1, email="test@test.com", hashed_password="hashed", role=Role(id=1, name="User"))
    mock_result.scalars.return_value.first.return_value = mock_user
    mock_session.execute.return_value = mock_result

    # Execute
    user = await repository.get_user_by_email("test@test.com")

    # Assert
    assert user is not None
    assert user.email == "test@test.com"
    assert user.role == "User"
    mock_session.execute.assert_called_once()


@pytest.mark.asyncio
async def test_get_user_by_email_not_exists(mock_session):
    # Setup mock
    repository = AuthRepository(mock_session)
    mock_result = MagicMock()
    mock_result.scalars.return_value.first.return_value = None
    mock_session.execute.return_value = mock_result

    # Execute
    user = await repository.get_user_by_email("notfound@test.com")

    # Assert
    assert user is None
    mock_session.execute.assert_called_once()


@pytest.mark.asyncio
async def test_create_user(mock_session):
    # Setup mock
    repository = AuthRepository(mock_session)
    from app.features.auth.dto import UserInDB
    user_data = UserInDB(email="new@test.com", hashed_password="hashed_password", role="Admin", full_name="New User")

    # Mock finding the role
    mock_role_result = MagicMock()
    mock_role_result.scalars.return_value.first.return_value = Role(id=1, name="Admin")

    # Mock finding the saved user with joined role
    mock_user_result = MagicMock()
    mock_user_result.scalars.return_value.first.return_value = User(id=1, email="new@test.com", full_name="New User", hashed_password="hashed_password", role=Role(id=1, name="Admin"))

    # Return different mocks for the two queries
    mock_session.execute.side_effect = [mock_role_result, mock_user_result]

    # Execute
    created_user = await repository.create_user(user_data)

    # Assert
    assert created_user.email == "new@test.com"
    assert created_user.role == "Admin"
    mock_session.add.assert_called_once()
    mock_session.commit.assert_called_once()
