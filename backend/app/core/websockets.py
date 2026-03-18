# backend/app/core/websockets.py
from fastapi import WebSocket
from typing import List, TypeVar, Any
import logging
from pydantic import BaseModel
import json

logger = logging.getLogger(__name__)

T = TypeVar('T')

class ConnectionManager:
    def __init__(self):
        # Lista em memória para guardar os clientes conectados (React, Kotlin)
        self.active_connections: List[WebSocket] = []

    async def connect(self, websocket: WebSocket):
        await websocket.accept()
        self.active_connections.append(websocket)
        logger.info(f"Novo cliente conectado. Total: {len(self.active_connections)}")

    def disconnect(self, websocket: WebSocket):
        if websocket in self.active_connections:
            self.active_connections.remove(websocket)
            logger.info(f"Cliente desconectado. Total restante: {len(self.active_connections)}")

    async def broadcast(self, data: T):
        """
        Envia dados estruturados (como um Pydantic model ou dict) para TODOS 
        os clientes conectados simultaneamente.
        """
        message_to_send: Any = data
        
        # Se for um modelo Pydantic, converte para dict serializável
        if isinstance(data, BaseModel):
            message_to_send = data.model_dump(mode="json")

        for connection in self.active_connections:
            try:
                await connection.send_json(message_to_send)
            except Exception as e:
                logger.error(f"Erro ao enviar mensagem via WS: {e}")
                # É uma boa prática tentar desconectar o cliente se o envio falhar
                self.disconnect(connection)

# Instância global do nosso "Hub"
websocket_manager = ConnectionManager()