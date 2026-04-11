# app/routes/__init__.py
from fastapi import APIRouter

# Importamos os roteadores das nossas features
from app.features.auth.controller import router as auth_router
from app.features.home.controller import router as home_router
from app.features.monitoring.controller import router as monitoring_router
from app.features.realtime.controller import router as realtime_router
from app.features.drain.controller import router as drain_router

# Criamos o roteador principal que agrupa todos
api_router = APIRouter()

# Registramos cada um. Note que você pode adicionar prefixos globais aqui se quiser.
api_router.include_router(auth_router)
api_router.include_router(home_router)
api_router.include_router(monitoring_router)
api_router.include_router(realtime_router)
api_router.include_router(drain_router)