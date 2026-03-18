import logging
from datetime import datetime, timezone

from app.features.monitoring.dto import SensorPayloadDTO, DrainStatusDTO
from app.features.monitoring.interfaces import IDrainRepository
from app.features.cache.service import RedisCacheService
from app.core.websockets import websocket_manager

logger = logging.getLogger(__name__)

class MonitoringService:
    MAX_BUCKET_DEPTH_CM: float = 120.0  
    CRITICAL_THRESHOLD_PERCENT: float = 80.0
    ALERT_THRESHOLD_PERCENT: float = 50.0

    def __init__(self, repository: IDrainRepository, cache_service: RedisCacheService) -> None:
        self._repository = repository
        self._cache_service = cache_service

    # Assinatura atualizada
    async def process_sensor_data(self, payload: SensorPayloadDTO) -> DrainStatusDTO:
        """
        Recebe o DTO direto do hardware, calcula a obstrução e salva via Repository.
        """
        distancia_lida = max(0.0, min(payload.distancia_cm, self.MAX_BUCKET_DEPTH_CM))
        espaco_ocupado_cm: float = self.MAX_BUCKET_DEPTH_CM - distancia_lida
        nivel_obstrucao: float = (espaco_ocupado_cm / self.MAX_BUCKET_DEPTH_CM) * 100.0

        status: str = "Normal"
        if nivel_obstrucao >= self.CRITICAL_THRESHOLD_PERCENT:
            status = "Crítico"
        elif nivel_obstrucao >= self.ALERT_THRESHOLD_PERCENT:
            status = "Alerta"

        resultado = DrainStatusDTO(
            id_bueiro=payload.id_bueiro, 
            distancia_cm=round(distancia_lida, 2),
            nivel_obstrucao=round(nivel_obstrucao, 2),
            status=status,
            latitude=payload.latitude,
            longitude=payload.longitude,
            ultima_atualizacao=datetime.now(timezone.utc)
        )

        await self._repository.save_sensor_data(resultado)

        # MÁGICA DO WEBSOCKET AQUI:
        # Se o bueiro estiver crítico ou em alerta, avisa todos os apps conectados
        if status in ["Crítico", "Alerta"]:
            evento = {
                "evento_tipo": "BUEIRO_STATUS_MUDOU",
                "dados": resultado.model_dump(mode="json") # Força a conversão de datetime para string
            }
            await websocket_manager.broadcast(evento)

        return resultado

    async def get_drain_status(self, bueiro_id: str) -> DrainStatusDTO:
        """
        Orquestra a busca do status do bueiro (Cache -> Banco de Dados).
        Dessa forma, tiramos essa responsabilidade do controller.
        """
        cache_key = f"bueiro:{bueiro_id}:status"

        async def fetch_fallback_from_db() -> DrainStatusDTO:
            status_db = await self._repository.get_latest_status(bueiro_id)
            if not status_db:
                raise ValueError("Bueiro não encontrado ou sem medições.")
            return status_db

        status_atual: DrainStatusDTO = await self._cache_service.get_or_set(
            key=cache_key,
            fetch_func=fetch_fallback_from_db,
            model_type=DrainStatusDTO,
            ttl_seconds=3600
        )
        return status_atual
