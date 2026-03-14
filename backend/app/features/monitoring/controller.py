from fastapi import APIRouter, Depends, HTTPException, Query, status

from .dto import AdafruitWebhookDTO, DrainStatusDTO
from .interfaces import IMonitoringService
from .service import MonitoringService
from .repository import DrainRepository

from app.core.database import get_db, AsyncClient
from app.core.cache import get_cache
from app.features.cache.service import RedisCacheService
from redis.asyncio import Redis
from app.core.config import settings

# Cria o roteador
router = APIRouter(
    prefix="/monitoring",
    tags=["Monitoramento de Bueiros"]
)

# ---------------------------------------------------------
# FÁBRICAS DE INJEÇÃO DE DEPENDÊNCIA (Dependency Injection)
# ---------------------------------------------------------
def get_cache_service(
    cache: Redis = Depends(get_cache)
) -> RedisCacheService:
    return RedisCacheService(redis_client=cache)

def get_monitoring_service(
    db: AsyncClient = Depends(get_db),
    cache: Redis = Depends(get_cache),
    cache_service: RedisCacheService = Depends(get_cache_service)
) -> MonitoringService: # Retornamos o concrete aqui devido à nova tipagem do orquestrador de status
    repository = DrainRepository(db_client=db, cache_client=cache)
    return MonitoringService(repository=repository, cache_service=cache_service)


# ---------------------------------------------------------
# ROTA 1: POST
# ---------------------------------------------------------
@router.post(
        "/webhook/{bueiro_id}",
        response_model=DrainStatusDTO,
        status_code=status.HTTP_200_OK)
async def adafruit_webhook(
    bueiro_id: str,
    payload: AdafruitWebhookDTO,
    token: str = Query(..., description="Token de segurança do Webhook"),
    service: MonitoringService = Depends(get_monitoring_service)
):
    if token != settings.ADAFRUIT_WEBHOOK_TOKEN:
        raise HTTPException(status_code=401, detail="Token de webhook inválido")

    try:
        return await service.process_sensor_data(bueiro_id, payload)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno: {str(e)}")


# ---------------------------------------------------------
# ROTA 2: GET
# ---------------------------------------------------------
@router.get(
        "/{bueiro_id}/status",
        response_model=DrainStatusDTO,
        status_code=status.HTTP_200_OK)
async def obter_status_bueiro(
    bueiro_id: str,
    service: MonitoringService = Depends(get_monitoring_service)
):
    """
    Retorna o status atual do bueiro, solicitando toda a lógica do Service
    """
    try:
        return await service.get_drain_status(bueiro_id)
    except ValueError as val_ex:
        # Se veio um ValueError do Service, significa que o repositório devolveu None
        raise HTTPException(status_code=404, detail=str(val_ex))
    except Exception as e:
        # Erro genérico de servidor 
        raise HTTPException(status_code=500, detail=f"Erro ao buscar status: {str(e)}")