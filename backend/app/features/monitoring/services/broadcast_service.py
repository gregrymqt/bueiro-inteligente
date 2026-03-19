# app/features/monitoring/services/broadcast_service.py
import logging
from app.core.websockets import websocket_manager
from app.features.monitoring.dto import DrainStatusDTO

logger = logging.getLogger(__name__)

class BroadcastService:
    async def enviar_atualizacao_bueiro(self, status_db: DrainStatusDTO) -> None:
        """
        Recebe o status confirmado do banco e dispara para os clientes conectados.
        """
        if status_db.status in ["Crítico", "Alerta"]:
            evento = {
                "evento_tipo": "BUEIRO_STATUS_MUDOU",
                # Força a conversão de datetime para string usando o Pydantic
                "dados": status_db.model_dump(mode="json") 
            }
            
            logger.info(f"Disparando broadcast para o bueiro {status_db.id_bueiro}")
            await websocket_manager.broadcast(evento)