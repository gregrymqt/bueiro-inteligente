# app/features/realtime/controller.py
from fastapi import APIRouter, WebSocket, WebSocketDisconnect
from app.extensions.realtime import realtime_extension
import logging

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/realtime", tags=["Realtime"])

@router.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    try:
        logger.info("Iniciando nova conexão WebSocket...")
        await realtime_extension.connect(websocket)
        logger.info("Conexão WebSocket estabelecida com sucesso pela extensão realtime.")
    except Exception as e:
        logger.error(f"Erro ao aceitar conexão WebSocket inicial: {str(e)}", exc_info=True)
        # Se a conexão inicial falhar, não temos como seguir o try
        return

    try:
        while True:
            # Mantém o túnel vivo
            await websocket.receive_text()
    except WebSocketDisconnect:
        logger.info("Cliente WebSocket desconectado normalmente (WebSocketDisconnect).")
        realtime_extension.disconnect(websocket)
    except Exception as e:
        logger.error(f"Erro inesperado no túnel WebSocket durante receive_text: {str(e)}", exc_info=True)
        realtime_extension.disconnect(websocket)