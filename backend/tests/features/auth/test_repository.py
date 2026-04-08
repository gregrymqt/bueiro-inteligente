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
    mock_role = Role(id=1, name="User")
    mock_user = User(id=1, email="test@test.com", full_name="Test User", hashed_password="hashed", role=mock_role)
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
    user_data = UserInDB(email="new@test.com", hashed_password="hashed_password", role="Admin")

    # Execute
    mock_role = Role(id=1, name="Admin")
    mock_result_role = MagicMock()
    mock_result_role.scalars.return_value.first.return_value = mock_role

    mock_result_user = MagicMock()
    mock_user = User(email="new@test.com", hashed_password="hashed_password", role=mock_role)
    mock_result_user.scalars.return_value.first.return_value = mock_user

    mock_session.execute.side_effect = [mock_result_role, mock_result_user]

    created_user = await repository.create_user(user_data)

    # Assert
    from unittest.mock import ANY
    assert created_user.email == "new@test.com"
    assert created_user.role == "Admin"
    mock_session.add.assert_called_once_with(ANY)
    mock_session.commit.assert_called_once()
