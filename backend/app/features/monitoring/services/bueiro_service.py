# app/features/monitoring/services/bueiro_service.py
import logging
from datetime import datetime, timezone

from app.features.monitoring.dto import SensorPayloadDTO, DrainStatusDTO
from app.features.monitoring.interfaces import IDrainRepository
from app.features.cache.service import RedisCacheService
from app.features.monitoring.services.broadcast_service import BroadcastService

logger = logging.getLogger(__name__)

class BueiroService:
    MAX_BUCKET_DEPTH_CM: float = 120.0  
    CRITICAL_THRESHOLD_PERCENT: float = 80.0
    ALERT_THRESHOLD_PERCENT: float = 50.0

    def __init__(
        self, 
        repository: IDrainRepository, 
        cache_service: RedisCacheService,
        broadcast_service: BroadcastService
    ) -> None:
        self._repository = repository
        self._cache_service = cache_service
        self._broadcast_service = broadcast_service

    async def process_sensor_data(self, payload: SensorPayloadDTO) -> DrainStatusDTO:
        """
        Recebe o DTO direto do hardware, calcula a obstrução e salva via Repository.
        """
        try:
            logger.info(f"Processando dados de sensor para o bueiro: {payload.id_bueiro}")
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

            # 1. Salva o dado fresco no Redis e o histórico no Supabase 
            await self._repository.save_sensor_data(resultado)

            # 2. Busca a medição mais recente no Supabase como garantia 
            status_confirmado_db = await self._repository.get_latest_status(payload.id_bueiro)
            
            # 3. Se encontrou no banco, dispara pelo socket para atualizar React e Kotlin
            if status_confirmado_db:
                await self._broadcast_service.enviar_atualizacao_bueiro(status_confirmado_db)

            logger.info(f"Dados processados com sucesso. Status atual: {status} (Bueiro: {payload.id_bueiro})")
            return resultado
        except Exception as e:
            logger.error(f"Erro ao processar dados do sensor para bueiro {payload.id_bueiro}: {str(e)}", exc_info=True)
            raise ValueError(f"Falha interna ao processar sensores: {str(e)}")

    async def get_drain_status(self, bueiro_id: str) -> DrainStatusDTO:
        """
        Orquestra a busca do status do bueiro (Cache -> Banco de Dados).
        Dessa forma, tiramos essa responsabilidade do controller.
        """
        try:
            logger.info(f"Buscando status do bueiro via Cache/DB: {bueiro_id}")
            cache_key = f"bueiro:{bueiro_id}:status"

            async def fetch_fallback_from_db() -> DrainStatusDTO:
                try:
                    logger.debug(f"Iniciando consulta fallback no Banco de Dados para bueiro {bueiro_id}")
                    status_db = await self._repository.get_latest_status(bueiro_id)
                    if not status_db:
                        logger.warning(f"Bueiro {bueiro_id} não encontrado ou sem medições na base")
                        raise ValueError("Bueiro não encontrado ou sem medições.")
                    return status_db
                except Exception as db_err:
                    logger.error(f"Erro no fallback do banco para bueiro {bueiro_id}: {str(db_err)}", exc_info=True)
                    raise

            response = await self._cache_service.get_or_set( 
                key=cache_key,
                fetch_func=fetch_fallback_from_db,
                model_type=DrainStatusDTO,
                ttl_seconds=3600
            )
            return response.data
        except ValueError:
            raise
        except Exception as e:
            logger.error(f"Erro ao tentar obter status do bueiro {bueiro_id}: {str(e)}", exc_info=True)
            raise ValueError(f"Falha desconhecida ao buscar status: {str(e)}")