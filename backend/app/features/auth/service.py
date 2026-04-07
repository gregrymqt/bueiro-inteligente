from fastapi import Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession
from .interfaces import IAuthService, IAuthRepository
from .repository import AuthRepository
from .dto import User, TokenPayload, UserCreate, UserInDB
from app.extensions.auth import auth_extension
from app.extensions.infrastructure import get_db
import logging

logger = logging.getLogger(__name__)

class AuthService(IAuthService):

    def __init__(self, repository: IAuthRepository):
        self.repository = repository

    async def authenticate_user(self, email: str, password: str) -> User | None:

        try:
            logger.info(f"Tentativa de autenticação para o usuário: {email}")
            user_in_db = await self.repository.get_user_by_email(email)       
            if not user_in_db:
                logger.warning(f"Autenticação falhou: usuário não encontrado ({email})")
                return None

            if not await auth_extension.verify_password(password, user_in_db.hashed_password):
                logger.warning(f"Autenticação falhou: senha inválida para o usuário ({email})")
                return None

            logger.info(f"Usuário autenticado com sucesso: {email}")
            return User(email=user_in_db.email, full_name=user_in_db.full_name, role=user_in_db.role)
        except Exception as e:
            logger.error(f"Erro inesperado ao autenticar usuário {email}: {str(e)}", exc_info=True)
            raise HTTPException(status_code=500, detail="Erro interno durante a autenticação")

    def create_access_token(self, user: User) -> str:
        try:
            logger.info(f"Gerando token de acesso para o usuário: {user.email}")
            payload = TokenPayload(sub=user.email, role=user.role)
            return auth_extension.create_access_token(payload)
        except Exception as e:
            logger.error(f"Erro ao gerar token de acesso para o usuário {user.email}: {str(e)}", exc_info=True)
            raise HTTPException(status_code=500, detail="Erro interno ao gerar token")

    async def logout(self, token_jti: str) -> None:

        try:
            logger.info(f"Iniciando logout do token jti: {token_jti}")
            await auth_extension.add_to_blacklist(token_jti)
            logger.info(f"Logout concluído para o token jti: {token_jti}")
        except Exception as e:
            logger.error(f"Erro ao processar logout para token jti {token_jti}: {str(e)}", exc_info=True)
            raise HTTPException(status_code=500, detail="Erro interno ao processar logout")
        
    async def register_user(self, user_create: UserCreate) -> User:

        try:
            logger.info(f"Iniciando registro do usuário: {user_create.email}")
            # 1. Verifica se já existe um usuário com este email
            existing_user = await self.repository.get_user_by_email(user_create.email)
            if existing_user:
                logger.warning(f"Tentativa de registro falhou: e-mail já existente ({user_create.email})")
                raise HTTPException(status_code=400, detail="Email already registered")

            # 2. Gera o hash da senha
            hashed_password = await auth_extension.get_password_hash(user_create.password)

            # 3. Monta o payload para o banco
            user_in_db = UserInDB(
                email=user_create.email,
                full_name=user_create.full_name,
                role=user_create.role,
                hashed_password=hashed_password
            )

            # 4. Salva usando o repositório
            saved_user = await self.repository.create_user(user_in_db)
            logger.info(f"Usuário registrado com sucesso: {saved_user.email}")
            
            # 5. Retorna o DTO sem o hash da senha
            return User(
                email=saved_user.email,
                full_name=saved_user.full_name,
                role=saved_user.role
            )
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Erro inesperado no registro do usuário {user_create.email}: {str(e)}", exc_info=True)
            raise HTTPException(status_code=500, detail="Erro interno ao registrar usuário")

# --- Dependency Injection Provider ---
def get_auth_service(db: AsyncSession = Depends(get_db)) -> AuthService:
    try:
        repository = AuthRepository(db)
        return AuthService(repository)
    except Exception as e:
        logger.error(f"Erro ao inicializar AuthService: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno de serviço")
