from .interfaces import IAuthService, IAuthRepository
from .dto import User
from backend.app.core.security import verify_password, create_access_token as create_token
from backend.app.core.blacklist import add_to_blacklist

class AuthService(IAuthService):
    """
    Implementação do serviço de autenticação.
    Contém a lógica de negócio para autenticar usuários e gerenciar tokens.
    """
    def __init__(self, repository: IAuthRepository):
        self.repository = repository

    async def authenticate_user(self, username: str, password: str) -> User | None:
        """
        Verifica as credenciais do usuário.
        1. Busca o usuário no repositório.
        2. Se o usuário existe, verifica se a senha fornecida corresponde ao hash armazenado.
        3. Se a senha for válida, retorna os dados do usuário.
        """
        user_in_db = await self.repository.get_user_by_username(username)
        if not user_in_db:
            return None
        
        if not await verify_password(password, user_in_db.hashed_password):
            return None
            
        return User(username=user_in_db.username, full_name=user_in_db.full_name)

    def create_access_token(self, user: User) -> str:
        """
        Cria um token de acesso JWT para o usuário fornecido.
        """
        return create_token(data={"sub": user.username})

    async def logout(self, token_jti: str) -> None:
        """
        Adiciona o JTI (identificador único) do token à blacklist para invalidá-lo.
        """
        await add_to_blacklist(token_jti)
