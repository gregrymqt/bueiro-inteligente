# app/features/monitoring/controller.py
from fastapi import APIRouter, Depends, HTTPException, status
from app.extensions.infrastructure import RoleChecker, get_db, get_cache
from app.features.cache.service import RedisCacheService
from app.features.monitoring.repository import DrainRepository
from app.features.monitoring.services.bueiro_service import BueiroService
from app.features.monitoring.services.broadcast_service import BroadcastService
from app.features.monitoring.dto import SensorPayloadDTO
from app.extensions.auth import verify_hardware_token

router = APIRouter(prefix="/monitoring", tags=["Monitoramento"])

# FÃ¡brica de injeÃ§Ã£o ajustada para a nova Service
def get_monitoring_service(
    db = Depends(get_db),
    cache = Depends(get_cache)
) -> BueiroService:
    cache_service = RedisCacheService(redis_client=cache)
    repository = DrainRepository(db_client=db, cache_service=cache_service)
    broadcast_service = BroadcastService()
    return BueiroService(repository, cache_service, broadcast_service)

@router.post("/medicoes", status_code=status.HTTP_200_OK)
async def receber_dados_sensor(
    payload: SensorPayloadDTO,
    _ = Depends(verify_hardware_token), # Controller nÃ£o vaza mais regra de token
    service: BueiroService = Depends(get_monitoring_service)
):
    try:
        return await service.process_sensor_data(payload)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/{bueiro_id}/status")
async def obter_status_bueiro(
    bueiro_id: str,
    service: BueiroService = Depends(get_monitoring_service),
    user = Depends(RoleChecker(allowed_roles=["admin", "manutencao", "cidadao"]))
):
    try:
        return await service.get_drain_status(bueiro_id)
    except ValueError as ve:
        raise HTTPException(status_code=404, detail=str(ve))
    except Exception as e:
        raise HTTPException(status_code=500, detail="Erro interno ao buscar status do bueiro.")
