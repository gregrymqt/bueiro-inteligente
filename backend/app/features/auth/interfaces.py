from abc import ABC, abstractmethod
from .dto import User, UserInDB

class IAuthRepository(ABC):
    """
    Interface para o repositório de autenticação.
    Define os métodos para interagir com a fonte de dados dos usuários.
    """
    @abstractmethod
    async def get_user_by_username(self, username: str) -> UserInDB | None:
        """Busca um usuário pelo seu nome de usuário (email)."""
        pass

class IAuthService(ABC):
    """
    Interface para o serviço de autenticação.
    Define a lógica de negócio para autenticação e gerenciamento de tokens.
    """
    @abstractmethod
    async def authenticate_user(self, username: str, password: str) -> User | None:
        """Autentica um usuário e, se bem-sucedido, retorna seus dados."""
        pass

    @abstractmethod
    def create_access_token(self, user: User) -> str:
        """Cria um token de acesso JWT para um usuário."""
        pass

    @abstractmethod
    async def logout(self, token_jti: str) -> None:
        """Invalida um token de acesso (logout)."""
        pass
