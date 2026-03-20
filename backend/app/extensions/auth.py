# app/extensions/auth.py
import logging
from datetime import datetime, timedelta, timezone
import uuid
from jose import jwt, JWTError
from passlib.context import CryptContext
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer

from app.core.config import settings
from app.extensions.infrastructure import infrastructure # Usamos a infraestrutura que jÃ¡ criamos
from app.features.auth.dto import UserTokenData

logger = logging.getLogger(__name__)

class AuthExtension:
    _instance = None

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
    
    def create_access_token(self, data: dict) -> str:
        to_encode = data.copy()
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

    # --- Gerenciamento de Blacklist (JWT Revocation) ---

    async def add_to_blacklist(self, jti: str):
        """Adiciona o ID do token Ã  blacklist no Redis para invalidar o logout."""
        redis = infrastructure.redis_client
        key = f"blacklist:{jti}"
        await redis.setex(name=key, time=int(self.blacklist_ttl), value="revoked")
        logger.info(f"Token {jti} adicionado Ã  blacklist (Logout realizado).")

    async def is_blacklisted(self, jti: str) -> bool:
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
        username: str = payload.get("sub")
        jti: str = payload.get("jti")
        roles: list[str] = payload.get("roles", [])

        if username is None or jti is None:
            raise credentials_exception
        
        # Verifica se o token foi revogado via logout 
        if await auth_extension.is_blacklisted(jti):
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token has been revoked",
                headers={"WWW-Authenticate": "Bearer"},
            )
        
        return UserTokenData(username=username, roles=roles, jti=jti)
            
    except JWTError:
        raise credentials_exception