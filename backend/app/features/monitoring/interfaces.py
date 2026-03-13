from typing import Protocol
from .dto import AdafruitWebhookDTO, DrainStatusDTO

class IDrainRepository(Protocol):
    async def save_sensor_data(self, data: DrainStatusDTO) -> None:
        """Contrato para persistir os dados do bueiro (Cache + DB)"""
        ...

class IMonitoringService(Protocol):
    async def process_sensor_data(self, bueiro_id: str, payload: AdafruitWebhookDTO) -> DrainStatusDTO:
        """Contrato para a regra de negócio de cálculo de obstrução"""
        ...