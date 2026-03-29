from .interfaces import IAuthService, IAuthRepository
from .dto import User, TokenPayload
from app.extensions.auth import auth_extension

class AuthService(IAuthService):
    """
    ImplementaÃ§Ã£o do serviÃ§o de autenticaÃ§Ã£o.
    ContÃ©m a lÃ³gica de negÃ³cio para autenticar usuÃ¡rios e gerenciar tokens.
    """
    def __init__(self, repository: IAuthRepository):
        self.repository = repository

    async def authenticate_user(self, username: str, password: str) -> User | None:
        """
        Verifica as credenciais do usuÃ¡rio.
        1. Busca o usuÃ¡rio no repositÃ³rio.
        2. Se o usuÃ¡rio existe, verifica se a senha fornecida corresponde ao hash armazenado.
        3. Se a senha for vÃ¡lida, retorna os dados do usuÃ¡rio.
        """
        user_in_db = await self.repository.get_user_by_username(username)
        if not user_in_db:
            return None

        if not await auth_extension.verify_password(password, user_in_db.hashed_password):
            return None

        return User(username=user_in_db.username, full_name=user_in_db.full_name, roles=user_in_db.roles)

    def create_access_token(self, user: User) -> str:
        # Passamos as roles do usuÃ¡rio para o payload do token
        payload = TokenPayload(sub=user.username, roles=user.roles)
        return auth_extension.create_access_token(payload)

    async def logout(self, token_jti: str) -> None:
        """
        Adiciona o JTI (identificador Ãºnico) do token Ã  blacklist para invalidÃ¡-lo.
        """
        await auth_extension.add_to_blacklist(token_jti)
