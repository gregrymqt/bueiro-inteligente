# backend/main.py
from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.extensions.infrastructure import infrastructure
from app.extensions.auth import auth_extension
from app.extensions.realtime import realtime_extension
from app.extensions.scheduler import scheduler_extension
from app.core.config import settings

# Importamos o agregador de rotas
from app.routes import api_router

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    await infrastructure.open()
    await auth_extension.open()
    await realtime_extension.open()
    await scheduler_extension.open()
    yield
    # Shutdown
    await scheduler_extension.close()
    await realtime_extension.close()
    await auth_extension.close()
    await infrastructure.close()

app = FastAPI(title="Bueiro Inteligente API", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.ALLOWED_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# O mapa de rotas centralizado
app.include_router(api_router)