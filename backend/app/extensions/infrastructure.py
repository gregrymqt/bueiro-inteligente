# app/extensions/infrastructure.py
import logging
import redis.asyncio as redis
from supabase import create_async_client, AsyncClient
from app.core.config import settings
from fastapi import Depends, HTTPException, status
from app.extensions.auth import get_current_user
from app.features.auth.dto import UserTokenData

logger = logging.getLogger(__name__)

class InfrastructureExtension:
    _instance = None

    def __new__(cls):
        """Implementação Singleton para garantir instância única."""
        if cls._instance is None:
            cls._instance = super(InfrastructureExtension, cls).__new__(cls)
            # Inicializamos os atributos como None (As declarações de tipo ficam implicadas em tempo de runtime)
            cls._instance.supabase = None 
            cls._instance.redis_client = None
        return cls._instance

    async def open(self):
        """Inicializa conexões com DB e Redis."""
        logger.info("Iniciando infraestrutura (Supabase & Redis)...")
        
        try:
            # 1. Configurando Supabase
            self.supabase = create_async_client(
                settings.SUPABASE_URL, 
                settings.SUPABASE_KEY
            )
            
            # 2. Configurando Redis
            redis_url = f"redis://{settings.REDIS_HOST}:{settings.REDIS_PORT}/{settings.REDIS_DB}"
            if settings.REDIS_PASSWORD:
                redis_url = f"redis://:{settings.REDIS_PASSWORD}@{settings.REDIS_HOST}:{settings.REDIS_PORT}/{settings.REDIS_DB}"
            
            self.redis_client = redis.from_url(
                redis_url, 
                decode_responses=True
            )
            
            # Teste simples de conexão (Ping)
            await self.redis_client.ping()
            logger.info("Conexões de infraestrutura estabelecidas com sucesso.")
            
        except Exception as e:
            logger.error(f"Falha crítica ao iniciar infraestrutura: {e}")
            raise e

    async def close(self):
        """Encerra graciosamente DB e Redis."""
        logger.info("Encerrando conexões de infraestrutura...")
        
        if self.redis_client:
            await self.redis_client.close()
            
        if self.supabase:
            # O cliente do Supabase usa httpx por baixo, fechamos a sessão
            await self.supabase.postgrest.aclose()
            
        logger.info("Infraestrutura encerrada.")

# Criamos o Singleton para ser exportado
infrastructure = InfrastructureExtension()

# ---------------------------------------------------------
# DEPENDÊNCIAS PARA INJEÇÃO (Usadas nos Controllers/Services)
# ---------------------------------------------------------
async def get_db() -> AsyncClient:
    return infrastructure.supabase

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
