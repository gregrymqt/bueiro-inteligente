# app/extensions/infrastructure.py
import logging
import redis.asyncio as redis
from sqlalchemy import text
from app.core.config import settings
from app.core.database import engine, SessionLocal

logger = logging.getLogger(__name__)

class InfrastructureExtension:
    _instance = None

    def __new__(cls):
        """Implementacao Singleton para garantir instancia unica."""
        if cls._instance is None:
            cls._instance = super(InfrastructureExtension, cls).__new__(cls)
            cls._instance.redis_client = None
            cls._instance.session_factory = None
        return cls._instance

    async def open(self):
        """Inicializa conexoes com o Redis e Banco de Dados."""
        logger.info("Iniciando infraestrutura (Redis e Banco de Dados)...")
        
        try:
            # Configurando Redis (simplificado)
            is_local = getattr(settings, "REDIS_LOCAL", True)
            
            if is_local:
                redis_url = "redis://redis:6379/0"
                connection_type = "Local (Dev)"
            else:
                redis_url = getattr(settings, "REDIS_URL", "")
                if redis_url and redis_url.startswith("redis://"):
                    redis_url = redis_url.replace("redis://", "rediss://", 1)
                connection_type = "Externa/Nuvem"

            logger.info(f"Iniciando conexao com o Redis. Estrategia de rede: {connection_type}")
            
            # Opcoes de conexao do Redis
            from typing import Any
            redis_options: dict[str, Any] = {"decode_responses": True}
            
            protocol_used = "rediss://" if redis_url.startswith("rediss://") else "redis://"
            logger.info(f"Conectando ao Redis utilizando o protocolo: {protocol_used}")
            
            # Configurando validacao do certificado em conexoes seguras
            if redis_url.startswith("rediss://"):
                redis_options["ssl_cert_reqs"] = "none"

            self.redis_client = redis.from_url(
                redis_url, 
                **redis_options
            )
            
            try:
                # Teste simples de conexao (Ping) garante a integridade apos a escolha
                await self.redis_client.ping() # type: ignore
                logger.info(f"Conexao com o Redis ({connection_type}) estabelecida com sucesso.")
            except redis.RedisError as e:
                logger.error(f"Falha ao conectar com o Redis ({connection_type}): {e}")
                raise e
            
            try:
                # Teste de conexao com o PostgreSQL
                async with engine.connect() as conn:
                    await conn.execute(text("SELECT 1"))
            except Exception as e:
                logger.error(f"Falha ao conectar com o PostgreSQL: {e}")
                raise e        
            
            # Atribuindo a fabrica de sessoes
            self.session_factory = SessionLocal
            logger.info("Conexao com o PostgreSQL estabelecida com sucesso.")
            
        except Exception as e:
            logger.error(f"Falha critica ao iniciar infraestrutura: {e}")
            raise e

    async def close(self):
        """Encerra graciosamente o Redis e Banco de Dados."""
        logger.info("Encerrando conexoes de infraestrutura...")
        
        if self.redis_client:
            await self.redis_client.close()
            logger.info("Conexao com o Redis encerrada.")
            
        # Encerramento do engine do SQLAlchemy
        await engine.dispose()
        logger.info("Conexao com o PostgreSQL encerrada.")
            
        logger.info("Infraestrutura encerrada.")

# Criamos o Singleton para ser exportado
infrastructure = InfrastructureExtension()

# ---------------------------------------------------------
# DEPENDENCIAS PARA INJECAO (Usadas nos Controllers/Services)
# ---------------------------------------------------------
async def get_db():
    """Injecao de dependencia para banco de dados."""
    if not infrastructure.session_factory:
        raise RuntimeError("Banco de dados nao finalizou a inicializacao. Session factory indisponivel.")
    async with infrastructure.session_factory() as db:
        yield db

async def get_cache() -> redis.Redis:
    return infrastructure.redis_client
