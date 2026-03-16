from pydantic import BaseModel, Field
from typing import Optional
from datetime import datetime

# Novo DTO de Entrada (O que o hardware envia)
class SensorPayloadDTO(BaseModel):
    id_bueiro: str = Field(..., example="B-01-CENTRO")
    distancia_cm: float = Field(..., description="Distância bruta em cm lida pelo sensor")
    latitude: Optional[float] = None
    longitude: Optional[float] = None

# DTO de Saída (O que vai para o React / Banco de Dados) - Mantido quase igual
class DrainStatusDTO(BaseModel):
    id_bueiro: str = Field(..., example="B-01-CENTRO")
    distancia_cm: float  
    nivel_obstrucao: float  
    status: str  
    latitude: Optional[float] = None
    longitude: Optional[float] = None
    ultima_atualizacao: datetime
    
    class Config:
        from_attributes = True
