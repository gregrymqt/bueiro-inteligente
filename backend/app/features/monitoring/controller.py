from fastapi import APIRouter, Depends, HTTPException, Query, WebSocket, WebSocketDisconnect, logger, status

from backend.app.core.roles import RoleChecker

from .dto import AdafruitWebhookDTO, DrainStatusDTO, SensorPayloadDTO
from .interfaces import IMonitoringService
from .service import MonitoringService
from .repository import DrainRepository

from app.core.database import get_db, AsyncClient
from app.core.cache import get_cache
from app.features.cache.service import RedisCacheService
from redis.asyncio import Redis
from app.core.config import settings
from app.core.websockets import websocket_manager

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
# ROTA DO HARDWARE (C++): Usa o Token do ESP32
# Não tem JWT aqui!
# ---------------------------------------------------------
@router.post("/medicoes", status_code=status.HTTP_200_OK)
async def receber_dados_sensor(
    payload: SensorPayloadDTO,
    token: str = Query(..., description="Token de segurança do Hardware"),
    service: MonitoringService = Depends(get_monitoring_service)
):
    if token != settings.HARDWARE_TOKEN:
        raise HTTPException(status_code=401, detail="Token de hardware inválido")
    
    return await service.process_sensor_data(payload)


# ---------------------------------------------------------
# ROTA 2: GET
# ---------------------------------------------------------
@router.get("/{bueiro_id}/status")
async def obter_status_bueiro(
    bueiro_id: str,
    service: MonitoringService = Depends(get_monitoring_service),
    # A MÁGICA AQUI: Só passa se for admin, manutencao ou cidadao logado
    user_logado: dict = Depends(RoleChecker(allowed_roles=["admin", "manutencao", "cidadao"]))
):
    """
    Retorna o status atual do bueiro. 
    A documentação do Swagger vai exigir o cadeado (Bearer Token) automaticamente!
    """
    return await service.get_drain_status(bueiro_id)
    

# ---------------------------------------------------------
# ROTA WEBSOCKET: Conexão em Tempo Real
# ---------------------------------------------------------
@router.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    """
    O React e o Kotlin vão se conectar nesta rota: ws://seuservidor/monitoring/ws
    """
    await websocket_manager.connect(websocket)
    try:
        while True:
            # O servidor fica aguardando, mantendo o túnel aberto
            data = await websocket.receive_text()
            logger.info(f"Recebido do WS: {data}")
    except WebSocketDisconnect:
        websocket_manager.disconnect(websocket)
