from pydantic import BaseModel, HttpUrl, ConfigDict
from typing import Literal, Optional, List
import uuid

# ==========================================
# Carousel DTOs
# ==========================================

class CarouselBase(BaseModel):
    title: str
    subtitle: Optional[str] = None
    image_url: HttpUrl
    action_url: Optional[HttpUrl] = None
    order: int
    section: Literal['hero', 'alerts', 'stats']

class CarouselCreateDTO(CarouselBase):
    """Payload para a criação de um novo item do Carousel."""
    pass

class CarouselUpdateDTO(BaseModel):
    """Payload para atualização de um item do Carousel."""
    title: Optional[str] = None
    subtitle: Optional[str] = None
    image_url: Optional[HttpUrl] = None
    action_url: Optional[HttpUrl] = None
    order: Optional[int] = None
    section: Optional[Literal['hero', 'alerts', 'stats']] = None

class CarouselDTO(CarouselBase):
    """Representação completa de um item do Carousel."""
    id: uuid.UUID
    model_config = ConfigDict(from_attributes=True)

# ==========================================
# StatCard DTOs
# ==========================================

class StatCardBase(BaseModel):
    title: str
    value: str
    description: str
    icon_name: str # Nome do ícone do Lucide
    color: Literal['success', 'warning', 'danger'] 
    order: int

class StatCardCreateDTO(StatCardBase):
    """Payload para a criação de um novo StatCard."""
    pass

class StatCardUpdateDTO(BaseModel):
    """Payload para atualização de um StatCard."""
    title: Optional[str] = None
    value: Optional[str] = None
    description: Optional[str] = None
    icon_name: Optional[str] = None
    color: Optional[Literal['success', 'warning', 'danger']] = None
    order: Optional[int] = None

class StatCardDTO(StatCardBase):
    """Representação completa de um StatCard."""
    id: uuid.UUID
    model_config = ConfigDict(from_attributes=True)

# ==========================================
# Home Responses
# ==========================================

class HomeResponse(BaseModel):
    carousels: List[CarouselDTO]
    stats: List[StatCardDTO]