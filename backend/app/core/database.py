import os
from dotenv import load_dotenv
from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker, AsyncSession
from sqlalchemy.orm import declarative_base
from sqlalchemy.pool import NullPool 

load_dotenv()

DATABASE_URL = os.getenv("DATABASE_URL")

if not DATABASE_URL:        
    raise ValueError("A variável de ambiente DATABASE_URL não está definida.")

# Garante o driver assíncrono para o SQLAlchemy
if DATABASE_URL.startswith("postgresql://"):
    DATABASE_URL = DATABASE_URL.replace("postgresql://", "postgresql+asyncpg://", 1)
elif DATABASE_URL.startswith("postgresql+psycopg2://"):
    DATABASE_URL = DATABASE_URL.replace("postgresql+psycopg2://", "postgresql+asyncpg://", 1)

# Cria a engine com as configurações para nuvem (Supabase/Render)
engine = create_async_engine(
    DATABASE_URL, 
    echo=True,
    # O NullPool é obrigatório ao usar o Transaction Pooler (porta 6543) do Supabase
    poolclass=NullPool, 
    # SSL deve ser 'ssl': True para conexões seguras do asyncpg com o Supabase
    connect_args={"ssl": True} 
)

SessionLocal = async_sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False,
    autocommit=False,
    autoflush=False
)

Base = declarative_base()

async def get_db():
    """Injeção de dependência para as rotas do FastAPI"""
    async with SessionLocal() as db:
        yield db