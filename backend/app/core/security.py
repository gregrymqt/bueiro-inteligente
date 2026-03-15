import asyncio
import uuid
from datetime import datetime, timedelta, timezone
from jose import jwt, JWTError
from passlib.context import CryptContext
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer

from backend.app.features.auth.dto import User
from .config import settings
from .blacklist import is_blacklisted

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token") # Este "token" será o nosso endpoint de login

def create_access_token(data: dict) -> str:
    to_encode = data.copy()
    expire = datetime.now(timezone.utc) + timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
    to_encode.update({
        "exp": expire,
        "iat": datetime.now(timezone.utc),
        "jti": str(uuid.uuid4()),  # Identificador único do token
    })
    return jwt.encode(to_encode, settings.SECRET_KEY, algorithm=settings.ALGORITHM)

async def verify_password(plain_password: str, hashed_password: str) -> bool:
    return await asyncio.to_thread(pwd_context.verify, plain_password, hashed_password)

async def get_password_hash(password: str) -> str:
    return await asyncio.to_thread(pwd_context.hash, password)

async def get_current_user(token: str = Depends(oauth2_scheme)):
    """
    Valida o token, verifica a blacklist e retorna o payload decodificado.
    Usado principalmente para o endpoint de logout.
    """
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    try:
        payload = jwt.decode(token, settings.SECRET_KEY, algorithms=[settings.ALGORITHM])
        username: str = payload.get("sub")
        jti: str = payload.get("jti")

        if username is None or jti is None:
            raise credentials_exception
        
        if await is_blacklisted(jti):
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token has been revoked",
                headers={"WWW-Authenticate": "Bearer"},
            )
        
        return payload # Retorna o payload para o logout usar o JTI
            
    except JWTError:
        raise credentials_exception

async def get_current_user_data(payload: dict = Depends(get_current_user)) -> User:
    """
    Dependência que valida o token E busca os dados do usuário no repositório.
    Retorna um DTO do usuário, garantindo que ele existe no sistema.
    """
    username = payload.get("sub")
    user_in_db = await mock_auth_repo.get_user_by_username(username)

    if user_in_db is None:
        raise HTTPException(status_code=404, detail="User not found")

    return User(username=user_in_db.username, full_name=user_in_db.full_name)