from pydantic import BaseModel, Field
from typing import Optional, Dict, Any
from datetime import datetime

# Para o Get Last Data (Imagem 2)
class AdafruitDataResponseDTO(BaseModel):
    id: str
    value: str  # O sensor envia como string, o Service converterá para float
    feed_id: int
    feed_key: str
    created_at: datetime
    lat: Optional[float] = None
    lon: Optional[float] = None
    ele: Optional[float] = None
    created_epoch: int

# Para o Webhook (Imagem 3)
class AdafruitWebhookDTO(BaseModel):
    value: str
    lat: Optional[float] = None
    lon: Optional[float] = None
    ele: Optional[float] = None



class DrainStatusDTO(BaseModel):
    id_bueiro: str = Field(..., example="B-01-CENTRO")
    distancia_cm: float  # Valor bruto lido pelo sensor
    nivel_obstrucao: float  # Cálculo: (distancia_atual / altura_total_bueiro) * 100
    status: str  # "Normal", "Alerta" ou "Crítico"
    latitude: Optional[float] = None
    longitude: Optional[float] = None
    ultima_atualizacao: datetime
    
    class Config:
        from_attributes = True  # Permite converter modelos do Supabase/ORM direto para DTO    