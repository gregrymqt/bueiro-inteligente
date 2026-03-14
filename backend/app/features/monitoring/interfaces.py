from typing import Protocol
from .dto import AdafruitWebhookDTO, DrainStatusDTO

class IDrainRepository(Protocol):
    async def save_sensor_data(self, data: DrainStatusDTO) -> None:
        """Contrato para persistir os dados do bueiro (Cache + DB)"""
        ...

    async def get_unsynced_data(self, limit: int = 100) -> list[DrainStatusDTO]:
        """Busca medições que ainda não foram enviadas ao Rows"""
        ...

    async def mark_as_synced(self, ids: list[str]) -> None:
        """Atualiza o status de sincronização no banco"""
        ...    

class IMonitoringService(Protocol):
    async def process_sensor_data(self, bueiro_id: str, payload: AdafruitWebhookDTO) -> DrainStatusDTO:
        """Contrato para a regra de negócio de cálculo de obstrução"""
        ...