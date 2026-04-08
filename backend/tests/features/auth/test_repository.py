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

    mock_role = MagicMock()
    mock_role.name = "User"
    mock_user = MagicMock()
    mock_user.email = "test@test.com"
    mock_user.full_name = "Test"
    mock_user.hashed_password = "pw"
    mock_user.role = mock_role

    mock_result = MagicMock()
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
    from app.features.auth.dto import UserInDB
    repository = AuthRepository(mock_session)

    # 1st execute: select Role
    mock_role = MagicMock()
    mock_role.id = 1

    # 2nd execute: select User to return fresh user
    mock_fresh_user = MagicMock()
    mock_fresh_user.email = "new@test.com"
    mock_fresh_user.full_name = "Test"
    mock_fresh_user.hashed_password = "pw"
    mock_fresh_user.role.name = "Admin"

    mock_result_role = MagicMock()
    mock_result_role.scalars.return_value.first.return_value = mock_role

    mock_result_user = MagicMock()
    mock_result_user.scalars.return_value.first.return_value = mock_fresh_user

    mock_session.execute.side_effect = [mock_result_role, mock_result_user]

    user_data = UserInDB(email="new@test.com", full_name="Test", role="Admin", hashed_password="pw")

    # Execute
    created_user = await repository.create_user(user_data)

    # Assert
    assert created_user.email == "new@test.com"
    assert mock_session.add.call_count == 1
    args, _ = mock_session.add.call_args
    assert isinstance(args[0], User)
    mock_session.commit.assert_called_once()
