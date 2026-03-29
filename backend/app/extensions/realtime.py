# app/extensions/realtime.py
import logging
from typing import List, Any, TypeVar
from fastapi import WebSocket
from pydantic import BaseModel

logger = logging.getLogger(__name__)
T = TypeVar('T')

class RealtimeExtension:
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(RealtimeExtension, cls).__new__(cls)
            # Lista em memória para guardar os clientes conectados 
            cls._instance.active_connections = [] 
        return cls._instance

    async def open(self):
        """Inicializa o Hub de conexÃµes em tempo real."""
        logger.info("Iniciando WebSocket Hub (Realtime)...")
        logger.info("WebSocket Hub pronto para receber conexÃµes.")

    async def close(self):
        """Fecha todas as conexÃµes ativas antes de desligar o servidor."""
        logger.info(f"Encerrando WebSocket Hub. Desconectando {len(self.active_connections)} clientes...")
        for connection in self.active_connections:
            try:
                await connection.close(code=1001, reason="Server shutting down")
            except Exception:
                pass
        self.active_connections.clear()
        logger.info("WebSocket Hub encerrado.")

    # --- MÃ©todos de Gerenciamento de ConexÃ£o ---

    async def connect(self, websocket: WebSocket):
        """Aceita a conexÃ£o e registra o cliente."""
        await websocket.accept()
        self.active_connections.append(websocket)
        logger.info(f"Novo cliente conectado. Total: {len(self.active_connections)} ")

    def disconnect(self, websocket: WebSocket):
        """Remove o cliente da lista quando ele desconecta."""
        if websocket in self.active_connections:
            self.active_connections.remove(websocket)
            logger.info(f"Cliente desconectado. Total restante: {len(self.active_connections)} ")

    async def broadcast(self, data: Any):
        """Envia dados para TODOS os clientes conectados simultaneamente."""
        message_to_send = data
        
        # Se for um modelo Pydantic, converte para dict serializÃ¡vel 
        if isinstance(data, BaseModel):
            message_to_send = data.model_dump(mode="json")

        for connection in list(self.active_connections):
            try:
                # Envia o JSON via socket 
                await connection.send_json(message_to_send)
            except Exception as e:
                logger.error(f"Erro ao enviar mensagem via WS: {e} ")
                self.disconnect(connection)

# Singleton para uso global
realtime_extension = RealtimeExtension()