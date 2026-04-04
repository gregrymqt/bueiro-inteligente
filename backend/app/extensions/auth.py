# app/extensions/auth.py
import logging
from datetime import datetime, timedelta, timezone
import uuid
from fastapi import Query
from jose import jwt, JWTError
from passlib.context import CryptContext
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer

from app.core.config import settings
from app.features.auth.dto import UserTokenData, TokenPayload

logger = logging.getLogger(__name__)

class AuthExtension:
    _instance = None
    pwd_context: CryptContext
    oauth2_scheme: OAuth2PasswordBearer
    blacklist_ttl: float

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(AuthExtension, cls).__new__(cls)
            cls._instance.pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")
            cls._instance.oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")
            # Blacklist TTL: Tempo de expiraÃ§Ã£o do token + 1 minuto de margem
            cls._instance.blacklist_ttl = timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES).total_seconds() + 60
        return cls._instance

    async def open(self):
        """Verifica se as configuraÃ§Ãµes de seguranÃ§a estÃ£o carregadas."""
        logger.info("Iniciando serviÃ§o de AutenticaÃ§Ã£o e SeguranÃ§a...")
        if not settings.SECRET_KEY or settings.SECRET_KEY == "mudar-depois":
            logger.warning("ALERTA: SECRET_KEY nÃ£o configurada ou insegura!")
        logger.info("ServiÃ§o de AutenticaÃ§Ã£o pronto.")

    async def close(self):
        logger.info("Encerrando serviÃ§o de AutenticaÃ§Ã£o.")

    # --- MÃ©todos de Core Auth ---
    
    def create_access_token(self, payload: TokenPayload) -> str:
        to_encode = payload.model_dump(exclude_unset=True)
        expire = datetime.now(timezone.utc) + timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
        to_encode.update({
            "exp": expire,
            "iat": datetime.now(timezone.utc),
            "jti": str(uuid.uuid4()),
        })
        return jwt.encode(to_encode, settings.SECRET_KEY, algorithm=settings.ALGORITHM)

    async def verify_password(self, plain_password: str, hashed_password: str) -> bool:
        import asyncio
        return await asyncio.to_thread(self.pwd_context.verify, plain_password, hashed_password)

    async def get_password_hash(self, password: str) -> str:
        """Gera o hash da senha usando bcrypt."""
        import asyncio
        return await asyncio.to_thread(self.pwd_context.hash, password)

    # --- Gerenciamento de Blacklist (JWT Revocation) ---

    async def add_to_blacklist(self, jti: str):
        """Adiciona o ID do token Ã  blacklist no Redis para invalidar o logout."""
        from app.extensions.infrastructure import infrastructure
        redis = infrastructure.redis_client
        key = f"blacklist:{jti}"
        await redis.setex(name=key, time=int(self.blacklist_ttl), value="revoked")
        logger.info(f"Token {jti} adicionado Ã  blacklist (Logout realizado).")

    async def is_blacklisted(self, jti: str) -> bool:
        from app.extensions.infrastructure import infrastructure
        redis = infrastructure.redis_client
        if not redis:
            return False
        return await redis.exists(f"blacklist:{jti}")

# InstÃ¢ncia Singleton
auth_extension = AuthExtension()

# --- Dependency Injection (O que vocÃª usarÃ¡ nos seus Controllers) ---

async def get_current_user(token: str = Depends(auth_extension.oauth2_scheme)) -> UserTokenData:
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    try:
        payload = jwt.decode(token, settings.SECRET_KEY, algorithms=[settings.ALGORITHM])
        email = payload.get("sub")
        jti = payload.get("jti")
        role = payload.get("role", "User") # Extraímos a string, default "User"

        if email is None or jti is None:
            raise credentials_exception

        # Verifica se o token foi revogado via logout
        if await auth_extension.is_blacklisted(jti):
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token has been revoked",
                headers={"WWW-Authenticate": "Bearer"},
            )

        return UserTokenData(email=email, role=role, jti=jti)
            
    except JWTError:
        raise credentials_exception


async def verify_hardware_token(token: str = Depends(OAuth2PasswordBearer(tokenUrl="token", auto_error=False)),
                                query_token: str = Query(None, alias="token")):
    """Verifica se a request foi feita pelo hardware seguro."""
    final_token = token or query_token
    if final_token != settings.HARDWARE_TOKEN:
        raise HTTPException(status_code=401, detail="Hardware inválido ou não autorizado")
    return final_token


from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from sqlalchemy.orm import joinedload
from app.core.database import get_db
from app.features.auth.models import User

class RoleChecker:
    """
    Verifica se a role do usuário (via Token JWT ou via BD) tem permissão de acessar a rota.
    Simula o comportamento do [Authorize(Roles="RoleName")] do ASP.NET Identity.
    """
    def __init__(self, allowed_roles: list[str], strict_db_check: bool = False):
        self.allowed_roles = allowed_roles
        self.strict_db_check = strict_db_check

    async def __call__(
        self, 
        current_user: UserTokenData = Depends(get_current_user),
        db: AsyncSession = Depends(get_db)
    ) -> UserTokenData:
        # Por padrão, confiamos no Claim contido no Payload do JWT de forma imediata
        user_role = current_user.role

        # Validação Opcional: Vai até o banco ver se a Role foi alterada ou revogada (Segurança Crítica)
        if self.strict_db_check:
            stmt = select(User).options(joinedload(User.role)).where(User.email == current_user.email)
            result = await db.execute(stmt)
            db_user = result.scalar_one_or_none()
            
            if not db_user or not db_user.role:
                raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Usuário ou role não encontrados no sistema.")
                
            user_role = db_user.role.name
            current_user.role = user_role # Atualiza o DTO com a role fresca

        # Verifica se o Role do usuário está na lista permitida
        if user_role not in self.allowed_roles:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Acesso negado: Esse recurso exige uma das roles {self.allowed_roles}, mas você possui a role '{user_role}'."
            )
            
        return current_user

