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
            # Configurando Redis (simplificado)
            is_local = getattr(settings, "REDIS_LOCAL", False)
            
            if is_local:
                redis_url = getattr(settings, "REDIS_EXTERNAL_URL", "redis://localhost:6379")
                connection_type = "Externa/Nuvem (Dev Local)"
            else:
                redis_url = getattr(settings, "REDIS_URL", "redis://localhost:6379")
                connection_type = "Interna/Render (Produção)"

            logger.info(f"Iniciando conexão com o Redis. Estratégia de rede: {connection_type}")
            
            # Opções de conexão do Redis
            from typing import Any
            redis_options: dict[str, Any] = {"decode_responses": True}
            
            protocol_used = "rediss://" if redis_url.startswith("rediss://") else "redis://"
            logger.info(f"Conectando ao Redis utilizando o protocolo: {protocol_used}")
            
            # Configurando validação do certificado em conexões seguras
            if redis_url.startswith("rediss://"):
                redis_options["ssl_cert_validation"] = "none"

            self.redis_client = redis.from_url(
                redis_url, 
                **redis_options
            )
            
            try:
                # Teste simples de conexão (Ping) garante a integridade após a escolha
                await self.redis_client.ping() # type: ignore
                logger.info(f"Conexão com o Redis ({connection_type}) estabelecida com sucesso.")
            except redis.RedisError as e:
                logger.error(f"Falha ao conectar com o Redis ({connection_type}): {e}")
                raise e
            
            try:
                # Teste de conexão com o PostgreSQL
                async with engine.connect() as conn:
                    await conn.execute(text("SELECT 1"))
            except Exception as e:
                logger.error(f"Falha ao conectar com o PostgreSQL: {e}")
                raise e        
            
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
