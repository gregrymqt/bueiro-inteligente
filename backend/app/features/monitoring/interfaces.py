from typing import Optional, Protocol
from .dto import SensorPayloadDTO, DrainStatusDTO # Atualizado aqui

class IDrainRepository(Protocol):
    async def save_sensor_data(self, data: DrainStatusDTO) -> None:
        ...
    async def get_unsynced_data(self, limit: int = 100) -> list[DrainStatusDTO]:
        ...
    async def mark_as_synced(self, ids: list[str]) -> None:
        ...    
    async def get_latest_status(self, bueiro_id: str) -> Optional[DrainStatusDTO]:
        ...

class IMonitoringService(Protocol):
    # Atualizado para receber apenas o SensorPayloadDTO
    async def process_sensor_data(self, payload: SensorPayloadDTO) -> DrainStatusDTO:
        ...
