from abc import ABC, abstractmethod
from .dto import User, UserInDB, UserCreate

class IAuthRepository(ABC):
    """
    Interface para o repositório de autenticação.
    Define os métodos para interagir com a fonte de dados dos usuários.
    """
    @abstractmethod
    async def get_user_by_email(self, email: str) -> UserInDB | None:     
        """Busca um usuÃ¡rio pelo seu email."""
        pass
    @abstractmethod
    async def create_user(self, user_in_db: UserInDB) -> UserInDB:
        """Persiste um novo usuario no banco de dados."""
        pass
class IAuthService(ABC):
    """
    Interface para o serviÃ§o de autenticaÃ§Ã£o.
    Define a lÃ³gica de negÃ³cio para autenticaÃ§Ã£o e gerenciamento de tokens. 
    """
    @abstractmethod
    async def authenticate_user(self, email: str, password: str) -> User | None:
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

    @abstractmethod
    async def register_user(self, user_create: UserCreate) -> User:
        """Registra um novo usuario e retorna seus dados base."""
        pass
