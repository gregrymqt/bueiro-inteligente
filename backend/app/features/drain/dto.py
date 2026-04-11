from pydantic import BaseModel, Field, ConfigDict
from typing import Optional
from datetime import datetime

class DrainBase(BaseModel):
    name: str = Field(..., description="Nome do bueiro")
    address: str = Field(..., description="Endereço do bueiro")
    latitude: float = Field(..., description="Latitude do bueiro")
    longitude: float = Field(..., description="Longitude do bueiro")
    is_active: bool = Field(default=True, description="Status de atividade do bueiro")

class DrainCreate(DrainBase):
    hardware_id: str = Field(..., description="ID único do hardware associado ao bueiro")

class DrainUpdate(BaseModel):
    name: Optional[str] = None
    address: Optional[str] = None
    latitude: Optional[float] = None
    longitude: Optional[float] = None
    is_active: Optional[bool] = None
    hardware_id: Optional[str] = None

class DrainRead(DrainBase):
    id: int
    hardware_id: str
    created_at: datetime

    model_config = ConfigDict(from_attributes=True)
