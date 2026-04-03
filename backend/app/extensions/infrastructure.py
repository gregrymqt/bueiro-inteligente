# app/extensions/infrastructure.py
import logging
import redis.asyncio as redis
from sqlalchemy import text
from app.core.config import settings
from fastapi import Depends, HTTPException, status
from app.extensions.auth import get_current_user
from app.features.auth.dto import UserTokenData
from app.core.database import engine, SessionLocal

logger = logging.getLogger(__name__)

class InfrastructureExtension:
    _instance = None

    def __new__(cls):
        """Implementação Singleton para garantir instância única."""
        if cls._instance is None:
            cls._instance = super(InfrastructureExtension, cls).__new__(cls)
            cls._instance.redis_client = None
            cls._instance.session_factory = None
        return cls._instance

    async def open(self):
        """Inicializa conexões com o Redis e Banco de Dados."""
        logger.info("Iniciando infraestrutura (Redis e Banco de Dados)...")
        
        try:
            # Configurando Redis
            # Usando getattr para evitar erro caso a senha não exista no settings
            redis_password = getattr(settings, "REDIS_PASSWORD", None)
            
            if redis_password:
                redis_url = f"redis://:{redis_password}@{settings.REDIS_HOST}:{settings.REDIS_PORT}/{settings.REDIS_DB}"
            else:
                redis_url = f"redis://{settings.REDIS_HOST}:{settings.REDIS_PORT}/{settings.REDIS_DB}"
            
            self.redis_client = redis.from_url(
                redis_url, 
                decode_responses=True
            )
            
            # Teste simples de conexão (Ping)
            await self.redis_client.ping()
            logger.info("Conexão com o Redis estabelecida com sucesso.")
            
            # Teste de conexão com o PostgreSQL
            async with engine.connect() as conn:
                await conn.execute(text("SELECT 1"))
            
            # Atribuindo a fábrica de sessões
            self.session_factory = SessionLocal
            logger.info("Conexão com o PostgreSQL estabelecida com sucesso.")
            
        except Exception as e:
            logger.error(f"Falha crítica ao iniciar infraestrutura: {e}")
            raise e

    async def close(self):
        """Encerra graciosamente o Redis e Banco de Dados."""
        logger.info("Encerrando conexões de infraestrutura...")
        
        if self.redis_client:
            await self.redis_client.close()
            logger.info("Conexão com o Redis encerrada.")
            
        # Encerramento do engine do SQLAlchemy
        await engine.dispose()
        logger.info("Conexão com o PostgreSQL encerrada.")
            
        logger.info("Infraestrutura encerrada.")

# Criamos o Singleton para ser exportado
infrastructure = InfrastructureExtension()

# ---------------------------------------------------------
# DEPENDÊNCIAS PARA INJEÇÃO (Usadas nos Controllers/Services)
# ---------------------------------------------------------
async def get_db():
    """Injeção de dependência para banco de dados."""
    if not infrastructure.session_factory:
        raise RuntimeError("Banco de dados não finalizou a inicialização. Session factory indísponivel.")
    async with infrastructure.session_factory() as db:
        yield db

async def get_cache() -> redis.Redis:
    return infrastructure.redis_client

class RoleChecker:
    """
    Dependência que verifica se o usuário logado possui uma das roles permitidas.
    """
    def __init__(self, allowed_roles: list[str]):
        self.allowed_roles = allowed_roles

    def __call__(self, user: UserTokenData = Depends(get_current_user)):
        if not any(role in user.roles for role in self.allowed_roles):
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="Você não tem permissão para acessar este recurso."
            )
        return user