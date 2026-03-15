from .interfaces import IAuthRepository
from .dto import UserInDB
from backend.app.core.security import get_password_hash

class MockAuthRepository(IAuthRepository):
    """
    Implementação de um repositório de autenticação em memória (mock).
    Ideal para desenvolvimento e testes, sem a necessidade de um banco de dados real.
    """
    def __init__(self):
        self._users_db = {}

    async def initialize(self):
        """
        Inicializa o "banco de dados" com um usuário de exemplo.
        """
        hashed_password = await get_password_hash("string")
        user_data = {
            "username": "user@example.com",
            "full_name": "User Example",
            "hashed_password": hashed_password,
        }
        user_in_db = UserInDB(**user_data)
        self._users_db[user_in_db.username] = user_in_db

    async def get_user_by_username(self, username: str) -> UserInDB | None:
        """
        Busca um usuário pelo nome de usuário no dicionário em memória.
        """
        return self._users_db.get(username)

# Instância singleton do repositório mock
mock_auth_repo = MockAuthRepository()
