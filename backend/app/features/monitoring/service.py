import logging
from datetime import datetime, timezone

# Correção dos imports absolutos para "app" em vez de "backend.app"
from app.features.monitoring.dto import AdafruitWebhookDTO, DrainStatusDTO
from app.features.monitoring.interfaces import IDrainRepository
from app.features.cache.service import RedisCacheService

logger = logging.getLogger(__name__)

class MonitoringService:
    # Variáveis estáticas de regra de negócio
    MAX_BUCKET_DEPTH_CM: float = 120.0  
    CRITICAL_THRESHOLD_PERCENT: float = 80.0
    ALERT_THRESHOLD_PERCENT: float = 50.0

    # Injeção de Dependências
    def __init__(self, repository: IDrainRepository, cache_service: RedisCacheService) -> None:
        self._repository = repository
        # Importante: Injetamos o serviço de cache aqui para orquestar as buscas na camada certa
        self._cache_service = cache_service

    async def process_sensor_data(self, bueiro_id: str, payload: AdafruitWebhookDTO) -> DrainStatusDTO:
        """
        Recebe o DTO bruto da Adafruit, calcula a obstrução e salva via Repository.
        """
        try:
            distancia_lida: float = float(payload.value)
        except ValueError:
            distancia_lida = self.MAX_BUCKET_DEPTH_CM 

        distancia_lida = max(0.0, min(distancia_lida, self.MAX_BUCKET_DEPTH_CM))
        espaco_ocupado_cm: float = self.MAX_BUCKET_DEPTH_CM - distancia_lida
        nivel_obstrucao: float = (espaco_ocupado_cm / self.MAX_BUCKET_DEPTH_CM) * 100.0

        status: str = "Normal"
        if nivel_obstrucao >= self.CRITICAL_THRESHOLD_PERCENT:
            status = "Crítico"
        elif nivel_obstrucao >= self.ALERT_THRESHOLD_PERCENT:
            status = "Alerta"

        resultado = DrainStatusDTO(
            id_bueiro=bueiro_id,
            distancia_cm=round(distancia_lida, 2),
            nivel_obstrucao=round(nivel_obstrucao, 2),
            status=status,
            latitude=payload.lat,
            longitude=payload.lon,
            ultima_atualizacao=datetime.now(timezone.utc)
        )

        await self._repository.save_sensor_data(resultado)
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
