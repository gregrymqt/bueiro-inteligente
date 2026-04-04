# app/features/monitoring/controller.py
from fastapi import APIRouter, Depends, HTTPException, status
from app.core.database import get_db
from app.extensions.infrastructure import get_cache
from app.features.cache.service import RedisCacheService
from app.features.monitoring.repository import DrainRepository
from app.features.monitoring.services.bueiro_service import BueiroService       
from app.features.monitoring.services.broadcast_service import BroadcastService 
from app.features.monitoring.dto import SensorPayloadDTO
from app.extensions.auth import verify_hardware_token, RoleChecker
from app.core.security import RateLimiter
import logging

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/monitoring", tags=["Monitoramento"])

# Fábrica de injeção ajustada para a nova Service
def get_monitoring_service(
    db = Depends(get_db),
    cache = Depends(get_cache)
) -> BueiroService:
    try:
        cache_service = RedisCacheService(redis_client=cache)
        repository = DrainRepository(db_client=db, cache_service=cache_service) 
        broadcast_service = BroadcastService()
        return BueiroService(repository, cache_service, broadcast_service)      
    except Exception as e:
        logger.error(f"Erro ao instanciar BueiroService: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno ao inicializar serviço")

@router.post("/medicoes", status_code=status.HTTP_200_OK, dependencies=[Depends(RateLimiter(times=5, seconds=10))])
async def receber_dados_sensor(
    payload: SensorPayloadDTO,
    _ = Depends(verify_hardware_token), # Controller não vaza mais regra de token
    service: BueiroService = Depends(get_monitoring_service)
):
    try:
        logger.info(f"Recepção de medição via sensor no bueiro: {payload.id_bueiro}")
        resultado = await service.process_sensor_data(payload)
        logger.info(f"Recepção concluída via controller com sucesso (Bueiro: {payload.id_bueiro})")
        return resultado
    except Exception as e:
        logger.error(f"Erro na rota POST /medicoes para {payload.id_bueiro}: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro ao registrar as medições recebidas.")

@router.get("/{bueiro_id}/status", dependencies=[Depends(RoleChecker(['User', 'Admin', 'Manager'])), Depends(RateLimiter(times=10, seconds=10))])
async def obter_status_bueiro(
    bueiro_id: str,
    service: BueiroService = Depends(get_monitoring_service)
):
    try:
        logger.info(f"Solicitando status do bueiro: {bueiro_id}")
        return await service.get_drain_status(bueiro_id)
    except ValueError as ve:
        logger.warning(f"Status não encontrado ou inválido para o bueiro {bueiro_id}: {str(ve)}")
        raise HTTPException(status_code=404, detail=str(ve))
    except Exception as e:
        logger.error(f"Erro não tratado na rota GET status para o bueiro {bueiro_id}: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail="Erro interno ao buscar status do bueiro.")
