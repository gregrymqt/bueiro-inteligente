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
    mock_user = User(id=1, email="test@test.com", role=Role.USER)
    mock_result.scalar_one_or_none.return_value = mock_user
    mock_session.execute.return_value = mock_result

    # Execute
    user = await repository.get_user_by_email("test@test.com")

    # Assert
    assert user is not None
    assert user.email == "test@test.com"
    assert user.role == Role.USER
    mock_session.execute.assert_called_once()


@pytest.mark.asyncio
async def test_get_user_by_email_not_exists(mock_session):
    # Setup mock
    repository = AuthRepository(mock_session)
    mock_result = MagicMock()
    mock_result.scalar_one_or_none.return_value = None
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
    user_data = User(email="new@test.com", hashed_password="hashed_password", role=Role.ADMIN)

    # Execute
    created_user = await repository.create_user(user_data)

    # Assert
    assert created_user.email == "new@test.com"
    assert created_user.role == Role.ADMIN
    mock_session.add.assert_called_once_with(user_data)
    mock_session.commit.assert_called_once()
    mock_session.refresh.assert_called_once_with(user_data)
