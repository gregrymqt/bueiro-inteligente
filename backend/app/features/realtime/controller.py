# app/features/realtime/controller.py
from fastapi import APIRouter, WebSocket, WebSocketDisconnect
from app.extensions.realtime import realtime_extension

router = APIRouter(prefix="/realtime", tags=["Realtime"])

@router.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await realtime_extension.connect(websocket)
    try:
        while True:
            # Mantém o túnel vivo
            await websocket.receive_text()
    except WebSocketDisconnect:
        realtime_extension.disconnect(websocket)