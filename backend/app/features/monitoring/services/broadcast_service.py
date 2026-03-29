# app/features/monitoring/services/broadcast_service.py
import logging
from app.extensions.realtime import realtime_extension
from app.features.monitoring.dto import DrainStatusDTO

logger = logging.getLogger(__name__)

class BroadcastService:
    async def enviar_atualizacao_bueiro(self, status_db: DrainStatusDTO) -> None:
        """
        Recebe o status confirmado do banco e dispara para os clientes conectados.
        """
        try:
            if status_db.status in ["Crítico", "Alerta"]:
                evento = {
                    "evento_tipo": "BUEIRO_STATUS_MUDOU",
                    # Força a conversão de datetime para string usando o Pydantic
                    "dados": status_db.model_dump(mode="json") 
                }
                
                logger.info(f"Disparando broadcast de alerta/crítico para o bueiro {status_db.id_bueiro}")
                await realtime_extension.broadcast(evento)
            else:
                logger.debug(f"Status Normal. Broadcast ignorado para o bueiro {status_db.id_bueiro}")
        except Exception as e:
            logger.error(f"Erro ao enviar broadcast socket para o bueiro {status_db.id_bueiro}: {str(e)}", exc_info=True)
            # Obs: Como erro em Broadcast não deve parar o salvamento da medição, não subimos o 'raise' aqui.