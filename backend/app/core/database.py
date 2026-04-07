import os
from dotenv import load_dotenv
from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker, AsyncSession
from sqlalchemy.orm import declarative_base
from sqlalchemy.pool import NullPool 
from app.core.config import settings
import ssl

load_dotenv()

DATABASE_URL = settings.DATABASE_URL.split('?')[0] # Remove query params da URL

if not DATABASE_URL:        
    raise ValueError("A variavel de ambiente DATABASE_URL não esta definida.")

connect_args = {}
if not getattr(settings, "DB_LOCAL", False):
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    connect_args["ssl"] = ctx

# Cria a engine com as configuracoes para nuvem (Supabase/Render)
engine = create_async_engine(
    DATABASE_URL, 
    echo=True,
    poolclass=NullPool, 
    connect_args=connect_args
)

SessionLocal = async_sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False,
    autocommit=False,
    autoflush=False
)

Base = declarative_base()

