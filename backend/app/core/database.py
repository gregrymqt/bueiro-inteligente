import os
from dotenv import load_dotenv
from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker, AsyncSession
from sqlalchemy.orm import declarative_base

# Carrega as variáveis do arquivo .env
load_dotenv()

# Lê a URL do banco de dados das variáveis de ambiente
DATABASE_URL = os.getenv("DATABASE_URL")

if not DATABASE_URL:
    raise ValueError("A variável de ambiente DATABASE_URL não está definida.")

# Garante o driver async
if DATABASE_URL.startswith("postgresql://"):
    DATABASE_URL = DATABASE_URL.replace("postgresql://", "postgresql+asyncpg://", 1)

# Cria a engine de conexão com o banco de dados
engine = create_async_engine(DATABASE_URL, echo=True)

# Cria a fábrica de sessões (SessionLocal)
SessionLocal = async_sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False,
    autocommit=False,
    autoflush=False
)

# Base declarativa para criação dos models
Base = declarative_base()

# Função de dependência para injetar a sessão do banco de dados nas rotas do FastAPI
async def get_db():
    async with SessionLocal() as db:
        yield db
