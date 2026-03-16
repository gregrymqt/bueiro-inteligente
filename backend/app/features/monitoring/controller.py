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
        "/medicoes", # Nova rota mais semântica
        response_model=DrainStatusDTO,
        status_code=status.HTTP_200_OK)
async def receber_dados_sensor(
    payload: SensorPayloadDTO, # Novo DTO
    token: str = Query(..., description="Token de segurança do Hardware"),
    service: MonitoringService = Depends(get_monitoring_service)
):
    # Dica: Atualize sua variável de ambiente de ADAFRUIT_WEBHOOK_TOKEN para HARDWARE_TOKEN
    if token != settings.HARDWARE_TOKEN:
        raise HTTPException(status_code=401, detail="Token de segurança inválido")

    try:
        # Passamos apenas o payload, o ID já está dentro dele
        return await service.process_sensor_data(payload)
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
